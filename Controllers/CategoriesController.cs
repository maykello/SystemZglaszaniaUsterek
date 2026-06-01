using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CategoriesController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;

        public CategoriesController(SystemZglaszaniaUsterekDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var items = await _context.Categories.OrderBy(c => c.Name).ToListAsync(ct);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View(new CategoryFormModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();
            if (await _context.Categories.AnyAsync(c => c.Name == name, ct))
            {
                ModelState.AddModelError(nameof(model.Name), "Kategoria o tej nazwie już istnieje.");
                return View(model);
            }

            _context.Categories.Add(new CategoryModel { Name = name });
            await _context.SaveChangesAsync(ct);
            TempData["CategoriesMessage"] = "Kategoria została dodana.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var item = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (item == null)
            {
                return NotFound();
            }
            return View(new CategoryFormModel { Id = item.Id, Name = item.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id, ct);
            if (item == null)
            {
                return NotFound();
            }

            var name = model.Name.Trim();
            if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != model.Id, ct))
            {
                ModelState.AddModelError(nameof(model.Name), "Kategoria o tej nazwie już istnieje.");
                return View(model);
            }

            item.Name = name;
            await _context.SaveChangesAsync(ct);
            TempData["CategoriesMessage"] = "Kategoria została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var item = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (item == null)
            {
                return NotFound();
            }

            var inUse = await _context.Tickets.AnyAsync(t => t.CategoryId == id, ct);
            if (inUse)
            {
                TempData["CategoriesError"] = "Nie można usunąć kategorii, która jest używana w zgłoszeniach.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(item);
            await _context.SaveChangesAsync(ct);
            TempData["CategoriesMessage"] = "Kategoria została usunięta.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class CategoryFormModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Nazwa jest wymagana.")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 1)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty;
    }
}
