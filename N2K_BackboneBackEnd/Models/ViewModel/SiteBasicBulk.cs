using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class SiteBasicBulk : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int N2KVersioningVersion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteBasicBulk>();
        }
    }
}