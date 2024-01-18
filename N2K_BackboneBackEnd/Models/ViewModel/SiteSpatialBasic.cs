using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class SiteSpatialBasic : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public Boolean? data { get; set; }
        public decimal? area { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteSpatialBasic>();
        }
    }
}