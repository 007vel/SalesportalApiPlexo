namespace PlexoRepPortal.Models
{
    public class RepDocument
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string Kind { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
