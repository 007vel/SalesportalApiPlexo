using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public UsersController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        private string? AdminEmail => _configuration["AdminLogin:Email"]?.Trim();
        private string? AdminPassword => _configuration["AdminLogin:Password"]?.Trim();

        // POST api/users/validate
        [HttpPost("validate")]
        public ActionResult<UserValidateResponse> Validate(UserValidateRequestForAdmin request)
        {
            var isValid = string.Equals(request.Email, AdminEmail, StringComparison.OrdinalIgnoreCase)
                && string.Equals(request.Password, AdminPassword, StringComparison.Ordinal);

            var response = new UserValidateResponse
            {
                IsValid = isValid,
                Message = isValid ? "Email and RepId match." : "Email and RepId do not match."
            };

            return Ok(response);
        }

        // POST api/users/rep-login-validate
        [HttpPost("rep-login-validate")]
        public async Task<ActionResult<UserValidateResponse>> RepLoginValidate(UserValidateRequest request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();
            var repId = request.RepId.Trim();

            var isValid = await _db.Reps
                .AsNoTracking()
                .AnyAsync(r => r.Email.ToLower() == email && r.RepId == repId, cancellationToken);

            var response = new UserValidateResponse
            {
                IsValid = isValid,
                Message = isValid ? "Email and RepId match." : "Email and RepId do not match."
            };

            return Ok(response);
        }
    }
}
