using System.ComponentModel.DataAnnotations;

namespace PlexoRepPortal.Models
{
    public class TrainingHubLinkRequest
    {
        [MaxLength(500)]
        public string? ProductVideoEnglishLink { get; set; }

        [MaxLength(500)]
        public string? ProductVideoSpanishLink { get; set; }

        [MaxLength(500)]
        public string? DashboardVideoEnglishLink { get; set; }

        [MaxLength(500)]
        public string? DashboardVideoSpanishLink { get; set; }

        [MaxLength(500)]
        public string? KnowledgeBaseLink { get; set; }

        [MaxLength(500)]
        public string? SalesLeadLink { get; set; }
    }
}
