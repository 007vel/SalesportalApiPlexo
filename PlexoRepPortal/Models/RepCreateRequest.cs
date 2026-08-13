using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepCreateRequest
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        [MaxLength(200)]
        public string? BusinessName { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [MaxLength(12), RegularExpression(@"^$|^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone must be in the format xxx-xxx-xxxx.")]
        public string? Phone { get; set; }

        public SalesRepType SalesRepType { get; set; } = SalesRepType.ReferralAgent;

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

        [MaxLength(500)]
        public string? PricingSheetLink { get; set; }

        [MaxLength(500)]
        public string? PowerPointLink { get; set; }

        public RepStatus Status { get; set; } = RepStatus.Pending;

        public DeleteStatus Delete { get; set; }
        public bool PassedCertification { get; set; }
        public bool BusinessCardsSent { get; set; }
        public bool ConsultantFeePaid { get; set; }
    }
}
