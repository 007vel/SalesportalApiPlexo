using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepBankDetailsRequest
    {
        [Required, MaxLength(30)]
        public string RepId { get; set; } = null!;

        [MaxLength(200)]
        public string? BankName { get; set; } = null!;

        [MaxLength(50)]
        public string? RoutingNumber { get; set; } = null!;

        [MaxLength(50)]
        public string? AccountNumber { get; set; } = null!;
    }
}
