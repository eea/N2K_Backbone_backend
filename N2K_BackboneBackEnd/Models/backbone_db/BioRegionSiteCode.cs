using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class BioRegionSiteCode : IEntityModel, IEntityModelBackboneDB
    {
        public string BioRegion { get; set; } = string.Empty;
        public string SiteCode { get; set; } = string.Empty;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegionSiteCode>();
        }
    }
}