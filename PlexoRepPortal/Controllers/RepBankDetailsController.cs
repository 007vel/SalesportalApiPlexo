using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;
using PlexoRepPortal.Services;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepBankDetailsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryption;

        public RepBankDetailsController(AppDbContext db, IEncryptionService encryption)
        {
            _db = db;
            _encryption = encryption;
        }

        // POST api/repbankdetails — creates or replaces the one bank-details record on file for a rep.
        [HttpPost]
        public async Task<ActionResult<RepBankDetailsDto>> Upsert(RepBankDetailsRequest request, CancellationToken cancellationToken)
        {
            var repExists = await _db.Reps.AnyAsync(r => r.RepId == request.RepId, cancellationToken);
            if (!repExists)
            {
                return NotFound($"Rep with RepId '{request.RepId}' was not found.");
            }

            var existing = await _db.RepBankDetails.FirstOrDefaultAsync(b => b.RepId == request.RepId, cancellationToken);
            var now = DateTime.UtcNow;

            if (existing is null)
            {
                existing = new RepBankDetails { RepId = request.RepId, CreatedAt = now };
                _db.RepBankDetails.Add(existing);
            }

            existing.BankName = _encryption.Encrypt(request.BankName);
            existing.RoutingNumber = _encryption.Encrypt(request.RoutingNumber);
            existing.AccountNumber = _encryption.Encrypt(request.AccountNumber);
            existing.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new RepBankDetailsDto
            {
                OId = existing.OId,
                RepId = existing.RepId,
                BankName = request.BankName,
                RoutingNumber = request.RoutingNumber,
                AccountNumber = request.AccountNumber,
                UpdatedAt = existing.UpdatedAt
            });
        }

        // GET api/repbankdetails/rep/1000 — bank name plus the full decrypted routing/account numbers.
        [HttpGet("rep/{repId}")]
        public async Task<ActionResult<RepBankDetailsDto>> GetByRep(string repId, CancellationToken cancellationToken)
        {
            var record = await _db.RepBankDetails.AsNoTracking().FirstOrDefaultAsync(b => b.RepId == repId, cancellationToken);
            if (record is null)
            {
                return NotFound();
            }

            return Ok(new RepBankDetailsDto
            {
                OId = record.OId,
                RepId = record.RepId,
                BankName = _encryption.Decrypt(record.BankName),
                RoutingNumber = _encryption.Decrypt(record.RoutingNumber),
                AccountNumber = _encryption.Decrypt(record.AccountNumber),
                UpdatedAt = record.UpdatedAt
            });
        }
    }
}
