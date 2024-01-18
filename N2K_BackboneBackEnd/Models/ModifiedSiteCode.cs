using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    public class ModifiedSiteCode
    {
        public string SiteCode { get; set; } = string.Empty;
        public int VersionId { get; set; }
        public SiteChangeStatus? Status { get; set; }
        public int? OK { get; set; }
        public string? Error { get; set; }
    }
}