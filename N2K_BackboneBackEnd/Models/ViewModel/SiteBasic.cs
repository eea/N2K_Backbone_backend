using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class SiteBasic : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Name { get; set; } = string.Empty;
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteBasic>();
        }
    }
}