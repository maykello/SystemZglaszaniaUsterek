using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var reporterId))
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

            return Redirect("/");
        }
    }
}
