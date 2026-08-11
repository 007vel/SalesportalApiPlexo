namespace PlexoRepPortal.Models
{
    /// Confirms a bank-details write/read. Routing/account numbers are decrypted and returned in
    /// full — the admin Rep Details page displays them directly, by explicit product decision.
    public class RepBankDetailsDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string RoutingNumber { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
