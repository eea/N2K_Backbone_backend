using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    [Keyless]
    public class SiteCodeVersion : IEntityModel, IEntityModelBackboneDB
    {

        public string SiteCode { get; set; } = "";
        public int Version { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteCodeVersion>();

        }
    }
}
