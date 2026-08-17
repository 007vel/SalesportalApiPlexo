namespace PlexoRepPortal.Models
{
    public class TrainingHubLinkDto
    {
        public int OId { get; set; }
        public string? ProductVideoEnglishLink { get; set; }
        public string? ProductVideoSpanishLink { get; set; }
        public string? DashboardVideoEnglishLink { get; set; }
        public string? DashboardVideoSpanishLink { get; set; }
        public string? KnowledgeBaseLink { get; set; }
        public string? SalesLeadLink { get; set; }

        public static TrainingHubLinkDto FromEntity(TrainingHubLink link) => new()
        {
            OId = link.OId,
            ProductVideoEnglishLink = link.ProductVideoEnglishLink,
            ProductVideoSpanishLink = link.ProductVideoSpanishLink,
            DashboardVideoEnglishLink = link.DashboardVideoEnglishLink,
            DashboardVideoSpanishLink = link.DashboardVideoSpanishLink,
            KnowledgeBaseLink = link.KnowledgeBaseLink,
            SalesLeadLink = link.SalesLeadLink
        };
    }
}
