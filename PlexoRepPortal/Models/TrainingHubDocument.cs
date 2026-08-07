namespace PlexoRepPortal.Models
{
    public class TrainingHubDocument
    {
        public int OId { get; set; }
        public string? RoleId { get; set; }
        public string Title { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string FileType { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? Length { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
