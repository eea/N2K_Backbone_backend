using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Sites : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public Boolean? Current { get; set; }
        public string? Name { get; set; }
        public DateTime? CompilationDate { get; set; }
        public DateTime? ModifyTS { get; set; }
        public int? CurrentStatus { get; set; }
        public string? CountryCode { get; set; }
        public string? SyteType { get; set; }
        public double? AltitudeMin { get; set; }
        public double? AltitudeMax { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public int? N2KVersioningRef { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sites>()
                .ToTable("Sites")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}
