namespace PlexoRepPortal.Models
{
    public class Rep
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? BusinessName { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public SalesRepType SalesRepType { get; set; } = SalesRepType.ReferralAgent;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? GoogleLink { get; set; }
        public string? ResourceLink { get; set; }
        public string? PricingSheetLink { get; set; }
        public string? PowerPointLink { get; set; }
        public RepStatus Status { get; set; } = RepStatus.Pending;
        public bool PassedCertification { get; set; }
        public BusinessCardStatus BusinessCardStatus { get; set; } = BusinessCardStatus.NotSent;
        public bool ConsultantFeePaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeleteStatus Delete { get; set; } = DeleteStatus.NotDeleted;

        /// Set when an admin manually sends the Rep welcome email and it actually goes out successfully;
        /// stays null until then, or if the send fails — see RepsController.SendWelcomeEmail.
        public DateTime? WelcomeEmailSentAt { get; set; }

        // ----- admin-only fields, set after creation from the Rep Details page -----
        public string? ContractWizardLink { get; set; }
        public string? ContractWizardUsername { get; set; }
        /// AES-encrypted ciphertext — see IEncryptionService. Never persisted or logged in plaintext.
        public string? ContractWizardPassword { get; set; }
        public string? ContractWizardInstructionsLink { get; set; }
        public string? PwrRewardsEmail { get; set; }
        /// AES-encrypted ciphertext — see IEncryptionService. Never persisted or logged in plaintext.
        public string? PwrRewardsEmailPassword { get; set; }
    }
}
