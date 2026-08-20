using System.Net;
using System.Net.Mail;
using PlexoCommon.Email;

namespace PlexoRepPortal.Services
{
    /// Sends HTML email via SMTP using settings from the "Smtp" config section.
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string? from, string subject, string body, CancellationToken cancellationToken = default)
        {
            var host = _configuration["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("Smtp:Host must be configured.");
            }

            // Callers that don't need a specific From (e.g. the rep welcome email) pass null and
            // rely on the configured default instead.
            var fromAddress = string.IsNullOrWhiteSpace(from) ? _configuration["Smtp:FromAddress"] : from;
            if (string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new InvalidOperationException("A From address is required — pass one explicitly or configure Smtp:FromAddress.");
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

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, string.IsNullOrWhiteSpace(from) ? _configuration["Smtp:FromName"] : null),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
