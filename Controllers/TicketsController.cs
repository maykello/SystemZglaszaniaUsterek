using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemZglaszaniaUsterek.Models.Enums;
using SystemZglaszaniaUsterek.Models.ViewModels;
using SystemZglaszaniaUsterek.Services;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var viewModel = new TicketCreateViewModel
            {
                Categories = await _ticketService.GetCategoriesAsync(ct)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _ticketService.GetCategoriesAsync(ct);
                return View(model);
            }

            if (!TryGetCurrentUser(out var reporterId, out _))
            {
                ModelState.AddModelError(string.Empty, "Nie udało się ustalić tożsamości zgłaszającego.");
                model.Categories = await _ticketService.GetCategoriesAsync(ct);
                return View(model);
            }

            var result = await _ticketService.CreateAsync(model, reporterId, ct);
            if (!result.Success)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
                model.Categories = await _ticketService.GetCategoriesAsync(ct);
                return View(model);
            }

            TempData["TicketActionMessage"] = "Zgłoszenie zostało utworzone.";
            return RedirectToAction(nameof(Details), new { id = result.TicketId });
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TicketFilterViewModel? filter, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            filter ??= new TicketFilterViewModel();
            var result = await _ticketService.ListAsync(filter, userId, role, ct);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            var details = await _ticketService.GetDetailsAsync(id, userId, role, ct);
            if (details == null)
            {
                return NotFound();
            }
            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Administrator")]
        public async Task<IActionResult> ChangeStatus(int id, int statusId, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            var result = await _ticketService.ChangeStatusAsync(id, statusId, userId, role, ct);
            TempData[result.Success ? "TicketActionMessage" : "TicketActionError"] =
                result.Success ? "Status zgłoszenia został zaktualizowany." : string.Join(" ", result.Errors);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Administrator")]
        public async Task<IActionResult> Assign(int id, int? technicianId, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            var result = await _ticketService.AssignTechnicianAsync(id, technicianId, userId, role, ct);
            TempData[result.Success ? "TicketActionMessage" : "TicketActionError"] =
                result.Success ? "Przydzielenie technika zostało zaktualizowane." : string.Join(" ", result.Errors);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Administrator")]
        public async Task<IActionResult> ChangePriority(int id, int priorityId, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            var result = await _ticketService.ChangePriorityAsync(id, priorityId, userId, role, ct);
            TempData[result.Success ? "TicketActionMessage" : "TicketActionError"] =
                result.Success ? "Priorytet zgłoszenia został zaktualizowany." : string.Join(" ", result.Errors);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddCommentViewModel model, CancellationToken ct)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["TicketActionError"] = "Treść komentarza jest niepoprawna.";
                return RedirectToAction(nameof(Details), new { id = model.TicketId });
            }

            var result = await _ticketService.AddCommentAsync(model.TicketId, model.Content, userId, role, ct);
            TempData[result.Success ? "TicketActionMessage" : "TicketActionError"] =
                result.Success ? "Komentarz został dodany." : string.Join(" ", result.Errors);

            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        [HttpGet]
        [Authorize(Roles = "Technician,Administrator")]
        public async Task<IActionResult> Notifications([FromQuery] long? sinceUnixMs, CancellationToken ct)
        {
            DateTime since;
            if (sinceUnixMs.HasValue)
            {
                since = DateTimeOffset.FromUnixTimeMilliseconds(sinceUnixMs.Value).UtcDateTime;
            }
            else
            {
                since = DateTime.UtcNow.AddSeconds(-60);
            }

            try
            {
                var items = await _ticketService.GetNewTicketsSinceAsync(since, ct);
                return Json(new
                {
                    serverTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    items = items.Select(i => new
                    {
                        id = i.Id,
                        title = i.Title,
                        reporter = i.ReporterUsername,
                        category = i.CategoryName,
                        createdAtUnixMs = new DateTimeOffset(DateTime.SpecifyKind(i.CreatedAtUtc, DateTimeKind.Utc)).ToUnixTimeMilliseconds()
                    })
                });
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new EmptyResult();
            }
        }

        private bool TryGetCurrentUser(out int userId, out Role role)
        {
            userId = 0;
            role = Role.User;

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out userId))
            {
                return false;
            }

            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            if (!Enum.TryParse<Role>(roleClaim, out role))
            {
                role = Role.User;
            }

            return true;
        }
    }
}
