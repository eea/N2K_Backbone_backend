using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class SiteBioRegionsAndArea : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? BioRegions { get; set; }
        public decimal? area { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteBioRegionsAndArea>();
        }
    }
}