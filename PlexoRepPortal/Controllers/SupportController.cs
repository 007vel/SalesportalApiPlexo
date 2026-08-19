using Microsoft.AspNetCore.Mvc;
using PlexoCommon.Email;
using PlexoCommon.Email.Templates;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public SupportController(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        // Fixed recipient, read from config rather than taken from the client, so it can't be redirected.
        private string ToMail => _configuration["Support:ToMail"]
            ?? throw new InvalidOperationException("Support:ToMail is not configured.");

        // POST api/support/ask-admin
        // Login page "Ask your admin" dialog — no rep identity available pre-login.
        [HttpPost("ask-admin")]
        public async Task<IActionResult> AskAdmin(AskAdminRequest request, CancellationToken cancellationToken)
        {
            var fields = new[] { ("Email", request.Email) };
            var html = AdminNotificationEmailTemplate.Build("Question from the Login Page", fields, request.Message);
            await _emailService.SendAsync(ToMail, request.Email, "Rep Portal – Question from the login page", html, cancellationToken);
            return Ok();
        }

        // POST api/support/contact-admin
        // Rep "Contact Us" page.
        [HttpPost("contact-admin")]
        public async Task<IActionResult> ContactAdmin(ContactAdminRequest request, CancellationToken cancellationToken)
        {
            var fields = new[] { ("Name", request.Name), ("Email", request.Email) };
            var html = AdminNotificationEmailTemplate.Build("Message from the Rep Portal", fields, request.Message);
            await _emailService.SendAsync(ToMail, request.Email, $"Rep Portal – Message from {request.Name}", html, cancellationToken);
            return Ok();
        }

        // POST api/support/business-card-request
        // Rep Profile -> Business Card -> "Request changes".
        [HttpPost("business-card-request")]
        public async Task<IActionResult> BusinessCardRequest(BusinessCardChangeRequest request, CancellationToken cancellationToken)
        {
            var fields = new[] { ("Rep ID", request.RepId), ("Email", request.Email) };
            var html = AdminNotificationEmailTemplate.Build("Business Card Change Request", fields, request.Notes);
            await _emailService.SendAsync(ToMail, request.Email, $"Rep Portal – Business Card Change Request (Rep {request.RepId})", html, cancellationToken);
            return Ok();
        }
    }
}
