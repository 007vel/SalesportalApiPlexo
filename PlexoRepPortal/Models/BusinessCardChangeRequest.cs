using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class BusinessCardChangeRequest
    {
        [Required, MaxLength(30)]
        public string RepId { get; set; } = null!;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(4000)]
        public string Notes { get; set; } = null!;
    }
}
