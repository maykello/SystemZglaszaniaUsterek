using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;
using SystemZglaszaniaUsterek.Services;

namespace SystemZglaszaniaUsterek.Controllers
{
    public class AuthController : Controller
    {
        private readonly SystemZglaszaniaUsterekDbContext _context;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(SystemZglaszaniaUsterekDbContext context, IPasswordResetService passwordResetService)
        {
            _context = context;
            _passwordResetService = passwordResetService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

                if (user != null && user.IsDeleted)
                {
                    ModelState.AddModelError(string.Empty, "Konto zostało dezaktywowane. Skontaktuj się z administratorem.");
                    return View(model);
                }

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Nieprawidłowa nazwa użytkownika lub hasło.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ping()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            return Json(new { ok = true, serverTimeUtc = DateTime.UtcNow });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var resetUrlBase = Url.Action(
                action: nameof(ResetPassword),
                controller: "Auth",
                values: null,
                protocol: Request.Scheme,
                host: Request.Host.Value)!;

            await _passwordResetService.RequestPasswordResetAsync(model.Email, resetUrlBase, ct);
            TempData["ForgotPasswordMessage"] =
                "Jeśli podany adres e-mail jest powiązany z aktywnym kontem, wysłaliśmy na niego link do ustawienia nowego hasła. " +
                "Sprawdź skrzynkę odbiorczą oraz folder SPAM.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || !_passwordResetService.IsTokenValid(token))
            {
                ViewData["TokenInvalid"] = true;
                return View();
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _passwordResetService.CompleteResetAsync(model.Token, model.NewPassword, ct);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Nie udało się ustawić nowego hasła.");
                return View(model);
            }

            TempData["LoginInfo"] = "Hasło zostało ustawione. Możesz się teraz zalogować.";
            return RedirectToAction(nameof(Login));
        }
    }
}
