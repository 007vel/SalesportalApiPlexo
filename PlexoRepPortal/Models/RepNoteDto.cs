namespace PlexoRepPortal.Models
{
    public class RepNoteDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string Kind { get; set; } = null!;
        public string? Text { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static RepNoteDto FromEntity(RepNote note) => new()
        {
            OId = note.OId,
            RepId = note.RepId,
            Kind = note.Kind,
            Text = note.Text,
            UpdatedAt = note.UpdatedAt
        };
    }
}
