namespace PlexoRepPortal.Models
{
    /// One bank-details record per Rep. Bank name/routing/account are stored AES-encrypted
    /// (see IEncryptionService) — never persisted or logged in plaintext.
    public class RepBankDetails
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string RoutingNumber { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
