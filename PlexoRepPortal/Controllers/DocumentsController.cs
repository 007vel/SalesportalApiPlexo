using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private const string StorageRoot = @"C:\pwr_docs";

        private readonly AppDbContext _db;

        public DocumentsController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/documents
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RepDocumentDto>> Upload([FromForm] string repId, [FromForm] string kind, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("A non-empty file is required.");
            }

            if (string.IsNullOrWhiteSpace(kind))
            {
                return BadRequest("Kind is required.");
            }

            var repExists = await _db.Reps.AnyAsync(r => r.RepId == repId, cancellationToken);
            if (!repExists)
            {
                return NotFound($"Rep with RepId '{repId}' was not found.");
            }

            Directory.CreateDirectory(StorageRoot);

            var originalFileName = Path.GetFileName(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            var filePath = Path.Combine(StorageRoot, storedFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var document = new RepDocument
            {
                RepId = repId,
                Kind = kind,
                FileName = originalFileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };

            _db.RepDocuments.Add(document);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { oId = document.OId }, RepDocumentDto.FromEntity(document));
        }

        // GET api/documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepDocumentDto>>> GetAll(CancellationToken cancellationToken)
        {
            var documents = await _db.RepDocuments
                .AsNoTracking()
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(cancellationToken);

            return Ok(documents.Select(RepDocumentDto.FromEntity));
        }

        // GET api/documents/rep/1000
        [HttpGet("rep/{repId}")]
        public async Task<ActionResult<IEnumerable<RepDocumentDto>>> GetByRep(string repId, CancellationToken cancellationToken)
        {
            var repExists = await _db.Reps.AnyAsync(r => r.RepId == repId, cancellationToken);
            if (!repExists)
            {
                return NotFound($"Rep with RepId '{repId}' was not found.");
            }

            var documents = await _db.RepDocuments
                .AsNoTracking()
                .Where(d => d.RepId == repId)
                .OrderBy(d => d.OId)
                .ToListAsync(cancellationToken);

            return Ok(documents.Select(RepDocumentDto.FromEntity));
        }

        // GET api/documents/5
        [HttpGet("{oId:int}")]
        public async Task<IActionResult> Get(int oId, CancellationToken cancellationToken)
        {
            var document = await _db.RepDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.OId == oId, cancellationToken);
            if (document is null)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(document.FilePath))
            {
                return NotFound("The stored file could not be found on disk.");
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(document.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // enableRangeProcessing lets clients request byte ranges (206 Partial Content) instead of
            // downloading the whole file up front — required for <video> to start playing/seeking before
            // it has fully downloaded. No fileDownloadName, so Content-Disposition isn't forced to
            // "attachment", letting the browser render the file inline.
            return PhysicalFile(document.FilePath, contentType, enableRangeProcessing: true);
        }

        // DELETE api/documents/5
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var document = await _db.RepDocuments.FirstOrDefaultAsync(d => d.OId == oId, cancellationToken);
            if (document is null)
            {
                return NotFound();
            }

            _db.RepDocuments.Remove(document);
            await _db.SaveChangesAsync(cancellationToken);

            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            return NoContent();
        }
    }
}
