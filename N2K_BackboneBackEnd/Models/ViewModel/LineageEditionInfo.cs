using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageEditionInfo : IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string SiteType { get; set; } = string.Empty;
        public string? BioRegion { get; set; }
        public double? AreaSDF { get; set; }
        public double? AreaGEO { get; set; }
        public double? Length { get; set; }
        public string? Status { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}