namespace PlexoRepPortal.Models
{
    public class RepDto
    {
        public int OId { get; set; }
        public string RepId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? GoogleLink { get; set; }
        public string? ResourceLink { get; set; }
        public RepStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// Ready-to-copy portal link, e.g. "plexopro.com/1000" — RepId itself always stays the plain number so rep login/lookup keeps matching on it.
        public string? PortalLink { get; set; }

        public static RepDto FromEntity(Rep rep, string? domain = null) => new()
        {
            OId = rep.OId,
            RepId = rep.RepId,
            FullName = rep.FullName,
            Email = rep.Email,
            Phone = rep.Phone,
            Address = rep.Address,
            City = rep.City,
            State = rep.State,
            Zip = rep.Zip,
            GoogleLink = rep.GoogleLink,
            ResourceLink = rep.ResourceLink,
            Status = rep.Status,
            CreatedAt = rep.CreatedAt,
            UpdatedAt = rep.UpdatedAt,
            PortalLink = string.IsNullOrEmpty(domain) ? null : $"{domain}/{rep.RepId}"
        };
    }
}
