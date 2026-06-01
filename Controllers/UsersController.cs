using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Enums;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;

        public UsersController(SystemZglaszaniaUsterekDbContext context)
        {
            _context = context;
        }

        // ----- Admin: list of users -----

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var users = await _context.Users.OrderBy(u => u.Username).ToListAsync(ct);
            return View(users);
        }

        // ----- Admin: create user -----

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(UserCreateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var trimmedUsername = model.Username.Trim();
            if (await _context.Users.AnyAsync(u => u.Username == trimmedUsername, ct))
            {
                ModelState.AddModelError(nameof(model.Username), "Nazwa użytkownika jest już zajęta.");
                return View(model);
            }

            var user = new UserModel
            {
                Username = trimmedUsername,
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? null : model.FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(model.LastName) ? null : model.LastName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            TempData["UserActionMessage"] = $"Użytkownik '{user.Username}' został utworzony.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(UserEditViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id, ct);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            user.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? null : model.FirstName.Trim();
            user.LastName = string.IsNullOrWhiteSpace(model.LastName) ? null : model.LastName.Trim();
            user.Role = model.Role;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync(ct);

            TempData["UserActionMessage"] = $"Dane użytkownika '{user.Username}' zostały zaktualizowane.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditRole(int id, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
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
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditRole(EditUserRoleViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId, ct);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = model.Role;
            await _context.SaveChangesAsync(ct);

            TempData["UserActionMessage"] = $"Rola użytkownika '{user.Username}' została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
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
                await _context.SaveChangesAsync(ct);
            }

            TempData["UserActionMessage"] = $"Konto użytkownika '{user.Username}' zostało dezaktywowane.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Profile(CancellationToken ct)
        {
            var user = await GetCurrentUserAsync(ct);
            if (user == null)
            {
                return Forbid();
            }

            var model = new ProfileEditViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileEditViewModel model, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync(ct);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                return View(model);
            }
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) ||
                    !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Aktualne hasło jest nieprawidłowe.");
                    model.Username = user.Username;
                    return View(model);
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            user.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            user.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? null : model.FirstName.Trim();
            user.LastName = string.IsNullOrWhiteSpace(model.LastName) ? null : model.LastName.Trim();

            await _context.SaveChangesAsync(ct);

            TempData["UserActionMessage"] = "Profil został zaktualizowany.";
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Auth");
            }

            return RedirectToAction(nameof(Profile));
        }

        private async Task<UserModel?> GetCurrentUserAsync(CancellationToken ct)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var id))
            {
                return null;
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        }
    }
}
