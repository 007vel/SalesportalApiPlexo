using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoCommon.Email;
using PlexoCommon.Email.Templates;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;
using PlexoRepPortal.Services;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepsController : ControllerBase
    {
        private const int RepIdStartingNumber = 1000;

        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryption;
        private readonly IEmailService _emailService;
        private readonly ILogger<RepsController> _logger;

        public RepsController(AppDbContext db, IConfiguration configuration, IEncryptionService encryption,
            IEmailService emailService, ILogger<RepsController> logger)
        {
            _db = db;
            _configuration = configuration;
            _encryption = encryption;
            _emailService = emailService;
            _logger = logger;
        }

        private string? Domain => _configuration["AppSettings:DomainName"]?.Trim();

        private string? BuildPortalLink(string repId) =>
            string.IsNullOrEmpty(Domain) ? null : $"{Domain!.TrimEnd('/')}/{repId}";

        private string? Encrypt(string? plaintext) =>
            string.IsNullOrEmpty(plaintext) ? null : _encryption.Encrypt(plaintext);

        // GET api/reps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepDto>>> GetAll(CancellationToken cancellationToken)
        {
            var reps = await _db.Reps
                .AsNoTracking()
                .OrderBy(r => r.OId)
                .ToListAsync(cancellationToken);

            return Ok(reps.Select(r => RepDto.FromEntity(r, Domain, _encryption)));
        }

        // GET api/reps/filter?status=Active&salesRepType=ReferralAgent&search=jordan
        // Every filter is optional and combines with AND. status/salesRepType bind from either the
        // enum's name or its numeric value; search does a case-insensitive substring match on FullName.
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<RepDto>>> Filter(
            [FromQuery] RepStatus? status,
            [FromQuery] SalesRepType? salesRepType,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var query = _db.Reps.AsNoTracking().AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (salesRepType.HasValue)
            {
                query = query.Where(r => r.SalesRepType == salesRepType.Value);
            }

            var trimmedSearch = search?.Trim();
            if (!string.IsNullOrEmpty(trimmedSearch))
            {
                query = query.Where(r => r.FullName.Contains(trimmedSearch));
            }

            var reps = await query.OrderBy(r => r.OId).ToListAsync(cancellationToken);

            return Ok(reps.Select(r => RepDto.FromEntity(r, Domain, _encryption)));
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

            return Ok(RepDto.FromEntity(rep, Domain, _encryption));
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
                BusinessName = request.BusinessName,
                Email = request.Email,
                Phone = request.Phone,
                SalesRepType = request.SalesRepType,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Zip = request.Zip,
                GoogleLink = request.GoogleLink,
                ResourceLink = request.ResourceLink,
                PricingSheetLink = request.PricingSheetLink,
                PowerPointLink = request.PowerPointLink,
                Status = request.Status,
                Delete = request.Delete,
                PassedCertification = request.PassedCertification,
                BusinessCardStatus = request.BusinessCardStatus,
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

            try
            {
                var loginLink = BuildPortalLink(rep.RepId);
                var (subject, html) = RepWelcomeEmailTemplate.Build(rep.FullName, rep.Email, rep.RepId, loginLink);
                await _emailService.SendAsync(rep.Email, from: null, subject, html, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Rep welcome email to {Email} for RepId {RepId}", rep.Email, rep.RepId);
            }

            var dto = RepDto.FromEntity(rep, Domain, _encryption);
            return CreatedAtAction(nameof(GetById), new { oId = rep.OId }, dto);
        }

        // RepId is stored as just the number (e.g. "1000") — PortalLink on the response is what
        // carries the domain-prefixed "plexopro.com/1000" form for display/copy purposes.
        private async Task<string> GenerateNextRepIdAsync(CancellationToken cancellationToken)
        {
            // IgnoreQueryFilters: deleting a rep is a soft delete (the row stays in the table), but
            // the unique index on RepId still enforces uniqueness against those rows — so the scan
            // has to see them too, or it'll happily hand out a RepId that's already taken.
            var existingRepIds = await _db.Reps
                .IgnoreQueryFilters()
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

            return Ok(RepDto.FromEntity(rep, Domain, _encryption));
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

            return Ok(RepDto.FromEntity(rep, Domain, _encryption));
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

            // IgnoreQueryFilters: same reasoning as GenerateNextRepIdAsync — the unique index on RepId
            // still enforces uniqueness against soft-deleted rows, so this check has to see them too.
            var repIdInUse = await _db.Reps.IgnoreQueryFilters().AnyAsync(r => r.RepId == request.RepId && r.OId != oId, cancellationToken);
            if (repIdInUse)
            {
                return Conflict($"RepId '{request.RepId}' is already in use.");
            }

            rep.RepId = request.RepId;
            rep.FullName = request.FullName;
            rep.BusinessName = request.BusinessName;
            rep.Email = request.Email;
            rep.Phone = request.Phone;
            rep.SalesRepType = request.SalesRepType;
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
            rep.BusinessCardStatus = request.BusinessCardStatus;
            rep.ConsultantFeePaid = request.ConsultantFeePaid;
            rep.ContractWizardLink = request.ContractWizardLink;
            rep.ContractWizardUsername = request.ContractWizardUsername;
            rep.ContractWizardPassword = Encrypt(request.ContractWizardPassword);
            rep.ContractWizardInstructionsLink = request.ContractWizardInstructionsLink;
            rep.PwrRewardsEmail = request.PwrRewardsEmail;
            rep.PwrRewardsEmailPassword = Encrypt(request.PwrRewardsEmailPassword);
            rep.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(RepDto.FromEntity(rep, Domain, _encryption));
        }

        // DELETE api/reps/5
        // Soft delete — flips DeleteStatus rather than removing the row, so the rep's documents/bank
        // details/commission history stay intact. The Rep entity's query filter then keeps it out of
        // GetAll/GetById/ValidateRepId/Update automatically.
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var rep = await _db.Reps.FirstOrDefaultAsync(r => r.OId == oId, cancellationToken);
            if (rep is null)
            {
                return NotFound();
            }

            rep.Delete = DeleteStatus.Deleted;
            rep.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
