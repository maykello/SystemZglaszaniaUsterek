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
    }
}
