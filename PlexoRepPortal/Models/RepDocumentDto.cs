namespace PlexoRepPortal.Models
{
    public class RepDocumentDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string Kind { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }

        public static RepDocumentDto FromEntity(RepDocument document) => new()
        {
            OId = document.OId,
            RepId = document.RepId,
            Kind = document.Kind,
            FileName = document.FileName,
            UploadedAt = document.UploadedAt
        };
    }
}
