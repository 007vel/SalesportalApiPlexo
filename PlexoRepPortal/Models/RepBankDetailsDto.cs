namespace PlexoRepPortal.Models
{
    /// Confirms a bank-details write/read without echoing back sensitive values — bank name is
    /// low-sensitivity so it's returned in full, but routing/account numbers are always masked
    /// (last 4 digits) so the UI can show "on file: ****1234" without ever exposing the real number.
    public class RepBankDetailsDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string MaskedRoutingNumber { get; set; } = null!;
        public string MaskedAccountNumber { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
