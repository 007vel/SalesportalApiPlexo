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
        private const string UploadedByAdmin = "Admin";
        private const string UploadedByRep = "Rep";
        private const string DefaultHubType = "Training";

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
            [FromForm] string? roleId,
            [FromForm] string title,
            [FromForm] IFormFile file,
            [FromForm] string uploadedBy,
            [FromForm] string? category,
            [FromForm] string? description,
            [FromForm] string? length,
            [FromForm] string Language,
            [FromForm] string? hubType,
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

            if (!string.Equals(uploadedBy, UploadedByAdmin, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uploadedBy, UploadedByRep, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"UploadedBy must be '{UploadedByRep}' or '{UploadedByAdmin}'.");
            }

            if (!string.IsNullOrWhiteSpace(hubType)
                && !string.Equals(hubType, "Training", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(hubType, "Marketing", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("HubType must be 'Training' or 'Marketing'.");
            }

            var isAdminUpload = string.Equals(uploadedBy, UploadedByAdmin, StringComparison.OrdinalIgnoreCase);
            var folderName = isAdminUpload ? UploadedByAdmin : SanitizeFolderName(roleId);
            var storageFolder = Path.Combine(StorageRoot, folderName);
            Directory.CreateDirectory(storageFolder);

            var originalFileName = Path.GetFileName(file.FileName);
            var fileType = ResolveFileType(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            var filePath = Path.Combine(storageFolder, storedFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var document = new TrainingHubDocument
            {
                RoleId = string.IsNullOrWhiteSpace(roleId) ? null : roleId,
                Title = title,
                Category = string.IsNullOrWhiteSpace(category) ? null : category,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                FileType = fileType,
                FileName = originalFileName,
                FilePath = filePath,
                Length = fileType == "Video" && !string.IsNullOrWhiteSpace(length) ? length : null,
                UploadedBy = string.Equals(uploadedBy, UploadedByAdmin, StringComparison.OrdinalIgnoreCase) ? UploadedByAdmin : UploadedByRep,
                UploadedAt = DateTime.UtcNow,
                Language =Language,
                HubType = NormalizeHubType(hubType),
            };

            _db.TrainingHubDocuments.Add(document);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { oId = document.OId }, TrainingHubDocumentDto.FromEntity(document));
        }

        // GET api/traininghub?hubType=Marketing
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingHubDocumentDto>>> GetAll([FromQuery] string? hubType, CancellationToken cancellationToken)
        {
            var resolvedHubType = NormalizeHubType(hubType);

            var documents = await _db.TrainingHubDocuments
                .AsNoTracking()
                .Where(d => d.HubType == resolvedHubType)
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

        // GET api/traininghub/filter?roleId=1000&includeAdmin=true
        // includeAdmin=true, roleId set    -> that role's documents + all Admin documents
        // includeAdmin=true, roleId empty  -> only Admin documents
        // includeAdmin=false               -> only that role's documents
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<TrainingHubDocumentDto>>> GetByRoleAndAdmin(
            [FromQuery] string? roleId,
            [FromQuery] bool includeAdmin,
            CancellationToken cancellationToken)
        {
            var trimmedRoleId = roleId?.Trim();

            var query = _db.TrainingHubDocuments.AsNoTracking();

            if (includeAdmin && string.IsNullOrEmpty(trimmedRoleId))
            {
                query = query.Where(d => d.UploadedBy == UploadedByAdmin);
            }
            else if (includeAdmin)
            {
                query = query.Where(d => d.RoleId == trimmedRoleId || d.UploadedBy == UploadedByAdmin);
            }
            else
            {
                query = query.Where(d => d.RoleId == trimmedRoleId);
            }

            var documents = await query
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

            // enableRangeProcessing lets clients request byte ranges (206 Partial Content) instead of
            // downloading the whole file up front — required for <video> to start playing/seeking before
            // it has fully downloaded. No fileDownloadName, so Content-Disposition isn't forced to
            // "attachment", letting the browser render the file inline.
            return PhysicalFile(document.FilePath, contentType, enableRangeProcessing: true);
        }

        // PUT api/traininghub/5
        // Edits a document's metadata, and optionally swaps its file. RoleId/UploadedBy/HubType are
        // ownership/identity fields set at creation and are never changed here.
        [HttpPut("{oId:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<TrainingHubDocumentDto>> Update(
            int oId,
            [FromForm] string title,
            [FromForm] string? category,
            [FromForm] string? description,
            [FromForm] string? length,
            [FromForm] string Language,
            [FromForm] IFormFile? file,
            CancellationToken cancellationToken)
        {
            var document = await _db.TrainingHubDocuments.FirstOrDefaultAsync(d => d.OId == oId, cancellationToken);
            if (document is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Title is required.");
            }

            string? oldFilePath = null;
            if (file is not null && file.Length > 0)
            {
                var isAdminUpload = document.UploadedBy == UploadedByAdmin;
                var folderName = isAdminUpload ? UploadedByAdmin : SanitizeFolderName(document.RoleId);
                var storageFolder = Path.Combine(StorageRoot, folderName);
                Directory.CreateDirectory(storageFolder);

                var originalFileName = Path.GetFileName(file.FileName);
                var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
                var newFilePath = Path.Combine(storageFolder, storedFileName);

                await using (var stream = System.IO.File.Create(newFilePath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                oldFilePath = document.FilePath;
                document.FileName = originalFileName;
                document.FilePath = newFilePath;
                document.FileType = ResolveFileType(originalFileName);
            }

            document.Title = title;
            document.Category = string.IsNullOrWhiteSpace(category) ? null : category;
            document.Description = string.IsNullOrWhiteSpace(description) ? null : description;
            document.Language = Language;
            document.Length = document.FileType == "Video" && !string.IsNullOrWhiteSpace(length) ? length : null;

            await _db.SaveChangesAsync(cancellationToken);

            // The old file is only deleted once the DB write has committed successfully, so a
            // mid-request failure never leaves the row pointing at a file that's already gone —
            // the opposite ordering from Delete, where the row is always what's being removed.
            if (oldFilePath is not null && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            return Ok(TrainingHubDocumentDto.FromEntity(document));
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

        private static string SanitizeFolderName(string? repId)
        {
            if (string.IsNullOrWhiteSpace(repId))
            {
                return "Unknown";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(repId.Trim().Where(c => !invalidChars.Contains(c)).ToArray());
            return sanitized.Length == 0 ? "Unknown" : sanitized;
        }

        private static string ResolveFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (VideoExtensions.Contains(extension)) return "Video";
            if (ImageExtensions.Contains(extension)) return "Image";
            if (extension == ".pdf") return "Pdf";
            return "Document";
        }

        // Canonicalizes a caller-supplied hub type to the exact casing stored in the database, defaulting to Training when omitted.
        private static string NormalizeHubType(string? hubType)
        {
            return string.Equals(hubType, "Marketing", StringComparison.OrdinalIgnoreCase) ? "Marketing" : DefaultHubType;
        }
    }
}
