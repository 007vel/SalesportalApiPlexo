namespace PlexoRepPortal.Models
{
    public class TrainingHubDocumentDto
    {
        public int OId { get; set; }
        public string RoleId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string FileType { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string? Length { get; set; }
        public DateTime UploadedAt { get; set; }

        public static TrainingHubDocumentDto FromEntity(TrainingHubDocument document) => new()
        {
            OId = document.OId,
            RoleId = document.RoleId,
            Title = document.Title,
            Category = document.Category,
            Description = document.Description,
            FileType = document.FileType,
            FileName = document.FileName,
            Length = document.Length,
            UploadedAt = document.UploadedAt
        };
    }
}
