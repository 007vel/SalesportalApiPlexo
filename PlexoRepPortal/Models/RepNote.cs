namespace PlexoRepPortal.Models
{
    /// One note record per (RepId, Kind) pair. Kind is "Admin" (visible to admin only) or
    /// "Shared" (visible to both the rep and admin) — see admin-reps-detials's Notes cards.
    public class RepNote
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string Kind { get; set; } = null!;
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
