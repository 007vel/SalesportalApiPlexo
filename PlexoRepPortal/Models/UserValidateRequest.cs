using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class UserValidateRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string RepId { get; set; } = null!;
    }
}
