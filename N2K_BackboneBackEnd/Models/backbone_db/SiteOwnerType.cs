using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteOwnerType : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int Type { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percent { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteOwnerType>()
                .ToTable("SiteOwnerType")
                .HasKey(c => new { c.SiteCode, c.Version, c.Type });
        }
    }
}
