using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class NutsBySite : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string NutId { get; set; } = string.Empty;
        public double? CoverPercentage { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsBySite>()
                .ToTable("NutsBySite")
                .HasKey(c => new { c.SiteCode, c.Version, c.NutId });
        }
    }
}
