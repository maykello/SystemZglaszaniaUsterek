using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PrioritiesController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;

        public PrioritiesController(SystemZglaszaniaUsterekDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var items = await _context.Priorities.OrderBy(p => p.Id).ToListAsync(ct);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View(new PriorityFormModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriorityFormModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();
            if (await _context.Priorities.AnyAsync(p => p.Name == name, ct))
            {
                ModelState.AddModelError(nameof(model.Name), "Priorytet o tej nazwie już istnieje.");
                return View(model);
            }

            _context.Priorities.Add(new PriorityModel { Name = name });
            await _context.SaveChangesAsync(ct);
            TempData["PrioritiesMessage"] = "Priorytet został dodany.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var item = await _context.Priorities.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (item == null)
            {
                return NotFound();
            }
            return View(new PriorityFormModel { Id = item.Id, Name = item.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PriorityFormModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = await _context.Priorities.FirstOrDefaultAsync(p => p.Id == model.Id, ct);
            if (item == null)
            {
                return NotFound();
            }

            var name = model.Name.Trim();
            if (await _context.Priorities.AnyAsync(p => p.Name == name && p.Id != model.Id, ct))
            {
                ModelState.AddModelError(nameof(model.Name), "Priorytet o tej nazwie już istnieje.");
                return View(model);
            }

            item.Name = name;
            await _context.SaveChangesAsync(ct);
            TempData["PrioritiesMessage"] = "Priorytet został zaktualizowany.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var item = await _context.Priorities.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (item == null)
            {
                return NotFound();
            }

            var inUse = await _context.Tickets.AnyAsync(t => t.Priority != null && t.Priority.Id == id, ct);
            if (inUse)
            {
                TempData["PrioritiesError"] = "Nie można usunąć priorytetu, który jest używany w zgłoszeniach.";
                return RedirectToAction(nameof(Index));
            }

            _context.Priorities.Remove(item);
            await _context.SaveChangesAsync(ct);
            TempData["PrioritiesMessage"] = "Priorytet został usunięty.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class PriorityFormModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Nazwa jest wymagana.")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 1)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty;
    }
}
