using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepNoteRequest
    {
        [Required, MaxLength(30)]
        public string RepId { get; set; } = null!;

        // "Admin" or "Shared" — see RepNote.
        [Required, MaxLength(20)]
        public string Kind { get; set; } = null!;

        public string? Text { get; set; }
    }
}
