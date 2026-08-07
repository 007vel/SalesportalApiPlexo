using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingHubController : ControllerBase
    {
        private const string StorageRoot = @"C:\pwr_docs\TrainingHub";

        private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".avi", ".webm", ".mkv", ".m4v" };
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".bmp" };

        private readonly AppDbContext _db;

        public TrainingHubController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/traininghub
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<TrainingHubDocumentDto>> Upload(
            [FromForm] string roleId,
            [FromForm] string title,
            [FromForm] IFormFile file,
            [FromForm] string? category,
            [FromForm] string? description,
            [FromForm] string? length,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("A non-empty file is required.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Title is required.");
            }

            Directory.CreateDirectory(StorageRoot);

            var originalFileName = Path.GetFileName(file.FileName);
            var fileType = ResolveFileType(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            var filePath = Path.Combine(StorageRoot, storedFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var document = new TrainingHubDocument
            {
                RoleId = roleId,
                Title = title,
                Category = string.IsNullOrWhiteSpace(category) ? null : category,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                FileType = fileType,
                FileName = originalFileName,
                FilePath = filePath,
                Length = fileType == "Video" && !string.IsNullOrWhiteSpace(length) ? length : null,
                UploadedAt = DateTime.UtcNow
            };

            _db.TrainingHubDocuments.Add(document);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { oId = document.OId }, TrainingHubDocumentDto.FromEntity(document));
        }

        // GET api/traininghub
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingHubDocumentDto>>> GetAll(CancellationToken cancellationToken)
        {
            var documents = await _db.TrainingHubDocuments
                .AsNoTracking()
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(cancellationToken);

            return Ok(documents.Select(TrainingHubDocumentDto.FromEntity));
        }

        // GET api/traininghub/role/1000
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<TrainingHubDocumentDto>>> GetByRole(string roleId, CancellationToken cancellationToken)
        {
            var documents = await _db.TrainingHubDocuments
                .AsNoTracking()
                .Where(d => d.RoleId == roleId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(cancellationToken);

            return Ok(documents.Select(TrainingHubDocumentDto.FromEntity));
        }

        // GET api/traininghub/5
        [HttpGet("{oId:int}")]
        public async Task<IActionResult> Get(int oId, CancellationToken cancellationToken)
        {
            var document = await _db.TrainingHubDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.OId == oId, cancellationToken);
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

            var bytes = await System.IO.File.ReadAllBytesAsync(document.FilePath, cancellationToken);
            return File(bytes, contentType, document.FileName);
        }

        // DELETE api/traininghub/5
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var document = await _db.TrainingHubDocuments.FirstOrDefaultAsync(d => d.OId == oId, cancellationToken);
            if (document is null)
            {
                return NotFound();
            }

            _db.TrainingHubDocuments.Remove(document);
            await _db.SaveChangesAsync(cancellationToken);

            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            return NoContent();
        }

        private static string ResolveFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (VideoExtensions.Contains(extension)) return "Video";
            if (ImageExtensions.Contains(extension)) return "Image";
            if (extension == ".pdf") return "Pdf";
            return "Document";
        }
    }
}
