using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DescribeSites : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;
        public decimal? Percentage { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DescribeSites>()
                .ToTable("DescribeSites")
                .HasKey(c => new { c.SiteCode, c.Version, c.HabitatCode });
        }
    }
}
