using Microsoft.AspNetCore.Mvc;
using PlexoRepPortal.Models;
using PlexoRepPortal.Services;

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

        // POST api/support/contact-admin
        [HttpPost("contact-admin")]
        public async Task<IActionResult> ContactAdmin(ContactAdminRequest request, CancellationToken cancellationToken)
        {
            await _emailService.SendAsync(ToMail, request.Email, "New message from the Rep Portal", request.Message, cancellationToken);
            return Ok();
        }
    }
}
