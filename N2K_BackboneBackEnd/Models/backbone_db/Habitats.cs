using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;
        public decimal? CoverHA { get; set; }
        public Boolean? PriorityForm { get; set; }
        public string? Representativity { get; set; }
        public int? DataQty { get; set; }
        public string? Conservation { get; set; }
        public string? GlobalAssesments { get; set; }
        public string? RelativeSurface { get; set; }
        public decimal? Percentage { get; set; }
        public string? ConsStatus { get; set; }
        public string? Caves { get; set; }
        public string? PF { get; set; }
        public string? NonPresenciInSite { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.id });
        }
    }
}
