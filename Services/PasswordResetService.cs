using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Services
{
    public class ResetPasswordResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }

        public static ResetPasswordResult Ok() => new() { Success = true };
        public static ResetPasswordResult Fail(string error) => new() { Success = false, Error = error };
    }

    public interface IPasswordResetService
    {
        Task RequestPasswordResetAsync(string email, string resetUrlBase, CancellationToken ct = default);

        bool IsTokenValid(string rawToken);

        Task<ResetPasswordResult> CompleteResetAsync(string rawToken, string newPassword, CancellationToken ct = default);
    }
    public class PasswordResetService : IPasswordResetService
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);
        private const string CacheKeyPrefix = "pwdreset:";

        private readonly SystemZglaszaniaUsterekDbContext _db;
        private readonly IEmailService _email;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            SystemZglaszaniaUsterekDbContext db,
            IEmailService email,
            IMemoryCache cache,
            ILogger<PasswordResetService> logger)
        {
            _db = db;
            _email = email;
            _cache = cache;
            _logger = logger;
        }

        public async Task RequestPasswordResetAsync(string email, string resetUrlBase, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            var trimmed = email.Trim();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email != null && u.Email == trimmed && !u.IsDeleted, ct);

            if (user == null)
            {
                _logger.LogInformation("Próba resetu hasła dla nieistniejącego/nieaktywnego e-maila: {Email}", trimmed);
                return;
            }

            var rawToken = GenerateRawToken();
            _cache.Set(CacheKey(rawToken), user.Id, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TokenLifetime
            });

            var separator = resetUrlBase.Contains('?') ? "&" : "?";
            var link = $"{resetUrlBase}{separator}token={Uri.EscapeDataString(rawToken)}";

            var subject = "System Zgłaszania Usterek — link do ustawienia nowego hasła";
            var body = $@"
<p>Cześć {System.Net.WebUtility.HtmlEncode(user.Username)},</p>
<p>Otrzymujesz tę wiadomość, ponieważ poprosiłeś/aś o przypomnienie hasła w systemie zgłaszania usterek.</p>
<p>Kliknij w link poniżej, aby ustawić nowe hasło. Link jest ważny przez {(int)TokenLifetime.TotalMinutes} minut i zadziała tylko raz.</p>
<p><a href=""{System.Net.WebUtility.HtmlEncode(link)}"" style=""display:inline-block;padding:10px 18px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:6px;font-weight:600"">Ustaw nowe hasło</a></p>
<p>Albo skopiuj ten adres do przeglądarki:<br/>
<span style=""font-family:monospace;color:#444"">{System.Net.WebUtility.HtmlEncode(link)}</span></p>
<p>Jeśli to nie Ty wysłałeś/aś prośbę, zignoruj tę wiadomość — żadne hasło nie zostanie zmienione bez kliknięcia w link.</p>
<hr/>
<p style=""color:#666;font-size:12px"">Wiadomość wygenerowana automatycznie. Nie odpowiadaj na nią.</p>";

            try
            {
                await _email.SendAsync(user.Email!, subject, body, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token zapisany w cache, ale e-mail z linkiem resetu nie został wysłany dla {Email}.", user.Email);
                _cache.Remove(CacheKey(rawToken));
            }
        }

        public bool IsTokenValid(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return false;
            }
            return _cache.TryGetValue(CacheKey(rawToken), out _);
        }

        public async Task<ResetPasswordResult> CompleteResetAsync(string rawToken, string newPassword, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return ResetPasswordResult.Fail("Hasło musi mieć co najmniej 6 znaków.");
            }

            if (string.IsNullOrWhiteSpace(rawToken) || !_cache.TryGetValue(CacheKey(rawToken), out var userIdObj) || userIdObj is not int userId)
            {
                return ResetPasswordResult.Fail("Link jest nieprawidłowy lub wygasł. Poproś o nowy.");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
            if (user == null)
            {
                _cache.Remove(CacheKey(rawToken));
                return ResetPasswordResult.Fail("Konto nie istnieje lub zostało dezaktywowane.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync(ct);

            _cache.Remove(CacheKey(rawToken));
            return ResetPasswordResult.Ok();
        }

        private static string CacheKey(string rawToken) => CacheKeyPrefix + rawToken;

        private static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
