using System.Net;
using System.Net.Mail;

namespace PlexoRepPortal.Services
{
    /// Sends plain-text email via SMTP using settings from the "Smtp" config section.
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string from, string subject, string body, CancellationToken cancellationToken = default)
        {
            var host = _configuration["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("Smtp:Host must be configured.");
            }
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentException("A From address is required.", nameof(from));
            }

            var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
            var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var parsedSsl) || parsedSsl;
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];

            using var client = new SmtpClient(host, port) { EnableSsl = enableSsl };
            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(user, password);
            }

            using var message = new MailMessage(from, to, subject, body);
            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
