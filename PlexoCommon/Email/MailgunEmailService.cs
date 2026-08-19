using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PlexoCommon.Email
{
    public class MailgunEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MailgunEmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string? from, string subject, string body, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["Mailgun:ApiKey"];
            var domain = _configuration["Mailgun:Domain"];
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(domain))
            {
                throw new InvalidOperationException("Mailgun:ApiKey and Mailgun:Domain must be configured.");
            }

            var fromAddress = string.IsNullOrWhiteSpace(from)
                ? $"{_configuration["Mailgun:FromName"]} <{_configuration["Mailgun:FromAddress"]}>"
                : from;

            var fields = new Dictionary<string, string>
            {
                ["from"] = fromAddress,
                ["to"] = to,
                ["subject"] = subject,
                ["html"] = body
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"v3/{domain}/messages")
            {
                Content = new FormUrlEncodedContent(fields)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}")));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Mailgun send failed ({(int)response.StatusCode}): {responseBody}");
            }
        }
    }
}
