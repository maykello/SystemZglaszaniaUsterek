using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SystemZglaszaniaUsterek.Models.Options;

namespace SystemZglaszaniaUsterek.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
            {
                _logger.LogError("SMTP nie jest skonfigurowany (brak Smtp:Username lub Smtp:Password). Email do {To} nie został wysłany.", toAddress);
                throw new InvalidOperationException("Konfiguracja SMTP jest niekompletna.");
            }

            var fromAddress = string.IsNullOrWhiteSpace(_options.FromAddress) ? _options.Username : _options.FromAddress;

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress!, _options.FromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };
            message.To.Add(new MailAddress(toAddress));

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            try
            {
                await client.SendMailAsync(message, ct);
                _logger.LogInformation("Wysłano e-mail do {To} (temat: {Subject})", toAddress, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nie udało się wysłać e-maila do {To}.", toAddress);
                throw;
            }
        }
    }
}
