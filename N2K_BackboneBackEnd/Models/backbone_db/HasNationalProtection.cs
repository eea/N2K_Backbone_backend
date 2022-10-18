using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class HasNationalProtection : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? DesignatedCode { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percentage { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HasNationalProtection>()
                .ToTable("HasNationalProtection")
                .HasKey(c => new { c.ID });
        }
    }
}
