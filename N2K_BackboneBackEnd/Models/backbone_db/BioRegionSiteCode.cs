using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    [Keyless]
    public class BioRegionSiteCode : IEntityModel, IEntityModelBackboneDB
    {
        public string BioRegion { get; set; } = "";
        public string SiteCode { get; set; } = "";

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegionSiteCode>();

        }

    }
}
