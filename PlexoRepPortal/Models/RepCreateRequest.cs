using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepCreateRequest
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? Zip { get; set; }

        [MaxLength(500)]
        public string? GoogleLink { get; set; }

        [MaxLength(500)]
        public string? ResourceLink { get; set; }

        public RepStatus Status { get; set; } = RepStatus.Pending;

        public bool PassedCertification { get; set; }
        public bool BusinessCardsSent { get; set; }
        public bool ConsultantFeePaid { get; set; }
    }
}
