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
        private const string DummyEmail = "sakthi@plexo.com";
        private const string DummyRepId = "1000";

        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/users/validate
        [HttpPost("validate")]
        public ActionResult<UserValidateResponse> Validate(UserValidateRequest request)
        {
            var isValid = string.Equals(request.Email, DummyEmail, StringComparison.OrdinalIgnoreCase)
                && string.Equals(request.RepId, DummyRepId, StringComparison.Ordinal);

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
