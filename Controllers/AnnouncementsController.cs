using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AnnouncementsController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;

        public AnnouncementsController(SystemZglaszaniaUsterekDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var items = await _context.Announcements
                .AsNoTracking()
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View(new AnnouncementFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var creator = await GetCurrentUserAsync(ct);

            var entity = new AnnouncementModel
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                Severity = model.Severity,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creator
            };

            _context.Announcements.Add(entity);
            await _context.SaveChangesAsync(ct);

            TempData["AnnouncementsMessage"] = "Ogłoszenie zostało dodane.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var entity = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (entity == null)
            {
                return NotFound();
            }

            var model = new AnnouncementFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Content = entity.Content,
                Severity = entity.Severity,
                IsActive = entity.IsActive
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AnnouncementFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == model.Id, ct);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Title = model.Title.Trim();
            entity.Content = model.Content.Trim();
            entity.Severity = model.Severity;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            TempData["AnnouncementsMessage"] = "Ogłoszenie zostało zaktualizowane.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, CancellationToken ct)
        {
            var entity = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (entity == null)
            {
                return NotFound();
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            TempData["AnnouncementsMessage"] = entity.IsActive
                ? "Ogłoszenie zostało aktywowane."
                : "Ogłoszenie zostało ukryte.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var entity = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (entity == null)
            {
                return NotFound();
            }

            _context.Announcements.Remove(entity);
            await _context.SaveChangesAsync(ct);

            TempData["AnnouncementsMessage"] = "Ogłoszenie zostało usunięte.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<UserModel?> GetCurrentUserAsync(CancellationToken ct)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var id))
            {
                return null;
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }
    }
}
