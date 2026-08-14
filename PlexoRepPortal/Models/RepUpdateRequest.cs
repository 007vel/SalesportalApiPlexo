using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepUpdateRequest
    {
        [Required, MaxLength(30)]
        public string RepId { get; set; } = null!;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        [MaxLength(200)]
        public string? BusinessName { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [MaxLength(12), RegularExpression(@"^$|^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone must be in the format xxx-xxx-xxxx.")]
        public string? Phone { get; set; }

        public SalesRepType SalesRepType { get; set; }

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

        public RepStatus Status { get; set; }

        public bool PassedCertification { get; set; }
        public bool BusinessCardsSent { get; set; }
        public bool ConsultantFeePaid { get; set; }

        // ----- admin-only fields, set after creation from the Rep Details page -----
        [MaxLength(500)]
        public string? ContractWizardLink { get; set; }

        [MaxLength(200)]
        public string? ContractWizardUsername { get; set; }

        /// Plaintext in transit (HTTPS) — encrypted by the controller before it's ever written to storage.
        [MaxLength(200)]
        public string? ContractWizardPassword { get; set; }

        [MaxLength(500)]
        public string? ContractWizardInstructionsLink { get; set; }

        [MaxLength(256)]
        public string? PwrRewardsEmail { get; set; }

        /// Plaintext in transit (HTTPS) — encrypted by the controller before it's ever written to storage.
        [MaxLength(200)]
        public string? PwrRewardsEmailPassword { get; set; }
    }
}
