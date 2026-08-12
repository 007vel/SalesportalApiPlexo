using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepsController : ControllerBase
    {
        private const int RepIdStartingNumber = 1000;

        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public RepsController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        private string? Domain => _configuration["AppSettings:DomainName"]?.Trim();

        // GET api/reps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepDto>>> GetAll(CancellationToken cancellationToken)
        {
            var reps = await _db.Reps
                .AsNoTracking()
                .OrderBy(r => r.OId)
                .ToListAsync(cancellationToken);

            return Ok(reps.Select(r => RepDto.FromEntity(r, Domain)));
        }

        // GET api/reps/5
        [HttpGet("{oId:int}")]
        public async Task<ActionResult<RepDto>> GetById(int oId, CancellationToken cancellationToken)
        {
            var rep = await _db.Reps.AsNoTracking().FirstOrDefaultAsync(r => r.OId == oId, cancellationToken);

            if (rep is null)
            {
                return NotFound();
            }

            return Ok(RepDto.FromEntity(rep, Domain));
        }

        // POST api/reps
        [HttpPost]
        public async Task<ActionResult<RepDto>> Create(RepCreateRequest request, CancellationToken cancellationToken)
        {
            var emailInUse = await _db.Reps.AnyAsync(r => r.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
            if (emailInUse)
            {
                return Conflict($"Email '{request.Email}' is already in use.");
            }

            var now = DateTime.UtcNow;
            var rep = new Rep
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Zip = request.Zip,
                GoogleLink = request.GoogleLink,
                ResourceLink = request.ResourceLink,
                PricingSheetLink = request.PricingSheetLink,
                PowerPointLink = request.PowerPointLink,
                Status = request.Status,
                PassedCertification = request.PassedCertification,
                BusinessCardsSent = request.BusinessCardsSent,
                ConsultantFeePaid = request.ConsultantFeePaid,
                CreatedAt = now,
                UpdatedAt = now
            };

            const int maxAttempts = 5;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                rep.RepId = await GenerateNextRepIdAsync(cancellationToken);
                _db.Reps.Add(rep);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                    break;
                }
                catch (DbUpdateException) when (attempt < maxAttempts)
                {
                    // RepId was taken by a concurrent request; detach and retry with the next number.
                    _db.Entry(rep).State = EntityState.Detached;
                }
            }

            var dto = RepDto.FromEntity(rep, Domain);
            return CreatedAtAction(nameof(GetById), new { oId = rep.OId }, dto);
        }

        // RepId is stored as just the number (e.g. "1000") — PortalLink on the response is what
        // carries the domain-prefixed "plexopro.com/1000" form for display/copy purposes.
        private async Task<string> GenerateNextRepIdAsync(CancellationToken cancellationToken)
        {
            var existingRepIds = await _db.Reps
                .Select(r => r.RepId)
                .ToListAsync(cancellationToken);

            var maxNumber = RepIdStartingNumber - 1;
            foreach (var repId in existingRepIds)
            {
                // Older rows may still carry a legacy "domain/number" RepId — parse past the slash so those don't skew the max.
                var slashIndex = repId.LastIndexOf('/');
                var numberPart = slashIndex >= 0 ? repId[(slashIndex + 1)..] : repId;
                if (int.TryParse(numberPart, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return (maxNumber + 1).ToString();
        }

        // GET api/reps/validate/1000
        [HttpGet("validate/{repId}")]
        public async Task<ActionResult<RepDto>> ValidateRepId(string repId, CancellationToken cancellationToken)
        {
            var trimmedRepId = repId.Trim();

            var rep = await _db.Reps
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RepId == trimmedRepId, cancellationToken);

            if (rep is null)
            {
                return NotFound(new { IsValid = false, Message = "RepId not found." });
            }

            return Ok(RepDto.FromEntity(rep, Domain));
        }

        // POST api/reps/link
        [HttpPost("link")]
        public async Task<ActionResult<RepDto>> UpdateLinks(RepLinkUpdateRequest request, CancellationToken cancellationToken)
        {
            var rep = await _db.Reps.FirstOrDefaultAsync(r => r.RepId ==Convert.ToString(request.RepsId), cancellationToken);
            if (rep is null)
            {
                return NotFound();
            }

            rep.GoogleLink = request.GoogleLink;
            rep.ResourceLink = request.ResourceLink;
            rep.PricingSheetLink = request.PricingSheetLink;
            rep.PowerPointLink = request.PowerPointLink;
            rep.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(RepDto.FromEntity(rep, Domain));
        }

        // PUT api/reps/5
        [HttpPut("{oId:int}")]
        public async Task<IActionResult> Update(int oId, RepUpdateRequest request, CancellationToken cancellationToken)
        {
            var rep = await _db.Reps.FirstOrDefaultAsync(r => r.OId == oId, cancellationToken);
            if (rep is null)
            {
                return NotFound();
            }

            var repIdInUse = await _db.Reps.AnyAsync(r => r.RepId == request.RepId && r.OId != oId, cancellationToken);
            if (repIdInUse)
            {
                return Conflict($"RepId '{request.RepId}' is already in use.");
            }

            rep.RepId = request.RepId;
            rep.FullName = request.FullName;
            rep.Email = request.Email;
            rep.Phone = request.Phone;
            rep.Address = request.Address;
            rep.City = request.City;
            rep.State = request.State;
            rep.Zip = request.Zip;
            rep.GoogleLink = request.GoogleLink;
            rep.ResourceLink = request.ResourceLink;
            rep.PricingSheetLink = request.PricingSheetLink;
            rep.PowerPointLink = request.PowerPointLink;
            rep.Status = request.Status;
            rep.PassedCertification = request.PassedCertification;
            rep.BusinessCardsSent = request.BusinessCardsSent;
            rep.ConsultantFeePaid = request.ConsultantFeePaid;
            rep.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(RepDto.FromEntity(rep, Domain));
        }

        // DELETE api/reps/5
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var rep = await _db.Reps.FirstOrDefaultAsync(r => r.OId == oId, cancellationToken);
            if (rep is null)
            {
                return NotFound();
            }

            _db.Reps.Remove(rep);
            await _db.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
