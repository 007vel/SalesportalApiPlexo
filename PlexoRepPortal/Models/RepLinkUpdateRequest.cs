using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class RepLinkUpdateRequest
    {
        [Required]
        public int RepsId { get; set; }

        [MaxLength(500)]
        public string? GoogleLink { get; set; }

        [MaxLength(500)]
        public string? ResourceLink { get; set; }

        [MaxLength(500)]
        public string? PricingSheetLink { get; set; }

        [MaxLength(500)]
        public string? PowerPointLink { get; set; }
    }
}
