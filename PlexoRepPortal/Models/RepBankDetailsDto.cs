namespace PlexoRepPortal.Models
{
    /// Confirms a bank-details write without echoing back sensitive values — only a masked
    /// account number (last 4 digits) so the UI can show "on file: ****1234".
    public class RepBankDetailsDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string MaskedAccountNumber { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
