using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class AskAdminRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(4000)]
        public string Message { get; set; } = null!;
    }
}
