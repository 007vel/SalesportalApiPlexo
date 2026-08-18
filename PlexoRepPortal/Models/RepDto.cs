using PlexoRepPortal.Services;

namespace PlexoRepPortal.Models
{
    public class RepDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? BusinessName { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public SalesRepType SalesRepType { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? GoogleLink { get; set; }
        public string? ResourceLink { get; set; }
        public string? PricingSheetLink { get; set; }
        public string? PowerPointLink { get; set; }
        public RepStatus Status { get; set; }
        public bool PassedCertification { get; set; }
        public BusinessCardStatus BusinessCardStatus { get; set; }
        public bool ConsultantFeePaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// Ready-to-copy portal link, e.g. "plexopro.com/1000" — RepId itself always stays the plain number so rep login/lookup keeps matching on it.
        public string? PortalLink { get; set; }

        // ----- admin-only fields, set after creation from the Rep Details page -----
        public string? ContractWizardLink { get; set; }
        public string? ContractWizardUsername { get; set; }
        /// Decrypted plaintext — the admin Rep Details page displays it directly, same as bank details.
        public string? ContractWizardPassword { get; set; }
        public string? ContractWizardInstructionsLink { get; set; }
        public string? PwrRewardsEmail { get; set; }
        /// Decrypted plaintext — the admin Rep Details page displays it directly, same as bank details.
        public string? PwrRewardsEmailPassword { get; set; }

        /// `encryption` decrypts ContractWizardPassword/PwrRewardsEmailPassword — omit it (e.g. for a
        /// context that shouldn't see credentials) and those two fields come back null instead.
        public static RepDto FromEntity(Rep rep, string? domain = null, IEncryptionService? encryption = null) => new()
        {
            OId = rep.OId,
            RepId = rep.RepId,
            FullName = rep.FullName,
            BusinessName = rep.BusinessName,
            Email = rep.Email,
            Phone = rep.Phone,
            SalesRepType = rep.SalesRepType,
            Address = rep.Address,
            City = rep.City,
            State = rep.State,
            Zip = rep.Zip,
            GoogleLink = rep.GoogleLink,
            ResourceLink = rep.ResourceLink,
            PricingSheetLink = rep.PricingSheetLink,
            PowerPointLink = rep.PowerPointLink,
            Status = rep.Status,
            PassedCertification = rep.PassedCertification,
            BusinessCardStatus = rep.BusinessCardStatus,
            ConsultantFeePaid = rep.ConsultantFeePaid,
            CreatedAt = rep.CreatedAt,
            UpdatedAt = rep.UpdatedAt,
            PortalLink = string.IsNullOrEmpty(domain) ? null : $"{domain.TrimEnd('/')}/{rep.RepId}",
            ContractWizardLink = rep.ContractWizardLink,
            ContractWizardUsername = rep.ContractWizardUsername,
            ContractWizardPassword = Decrypt(rep.ContractWizardPassword, encryption),
            ContractWizardInstructionsLink = rep.ContractWizardInstructionsLink,
            PwrRewardsEmail = rep.PwrRewardsEmail,
            PwrRewardsEmailPassword = Decrypt(rep.PwrRewardsEmailPassword, encryption)
        };

        private static string? Decrypt(string? ciphertext, IEncryptionService? encryption) =>
            string.IsNullOrEmpty(ciphertext) || encryption is null ? null : encryption.Decrypt(ciphertext);
    }
}
