using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;

        public UsersController(SystemZglaszaniaUsterekDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.OrderBy(u => u.Username).ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult EditRole(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditRole(EditUserRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                user.Role = model.Role;
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentIdClaim, out var currentId) && currentId == user.Id)
            {
                TempData["UserActionError"] = "Nie możesz usunąć własnego konta.";
                return RedirectToAction(nameof(Index));
            }

            if (!user.IsDeleted)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }

            TempData["UserActionMessage"] = $"Konto użytkownika '{user.Username}' zostało dezaktywowane.";
            return RedirectToAction(nameof(Index));
        }
    }
}
