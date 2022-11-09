using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? CoverHA { get; set; }
        public Boolean? PriorityForm { get; set; }
        public string? Representativity { get; set; }
        public int? DataQty { get; set; }
        public string? Conservation { get; set; }
        public string? GlobalAssesments { get; set; }
        public string? RelativeSurface { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percentage { get; set; }
        public string? ConsStatus { get; set; }
        public string? Caves { get; set; }
        public string? PF { get; set; }
        public int? NonPresenciInSite { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.id });
        }
    }
}
