using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DetailedProtectionStatus : IEntityModel, IEntityModelBackboneDB
    {
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? DesignationCode { get; set; }
        public string? Name { get; set; }
        public long ID { get; set; }
        public string? OverlapCode { get; set; }
        public decimal? OverlapPercentage { get; set; }
        public string? Convention { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<DetailedProtectionStatus>()
                .Property(b => b.OverlapPercentage)
                .HasPrecision(38, 2);

            builder.Entity<DetailedProtectionStatus>()
                .ToTable("DetailedProtectionStatus")
                .HasKey(c => new { c.ID });
        }
    }
}
