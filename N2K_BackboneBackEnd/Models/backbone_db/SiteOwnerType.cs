using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteOwnerType : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int Type { get; set; }
        public decimal? Percent { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteOwnerType>()
                .ToTable("SiteOwnerType")
                .HasKey(c => new { c.SiteCode, c.Version, c.Type });
        }
    }
}
