using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
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
        public SiteChangeStatus? CurrentStatus { get; set; }
        public string? CountryCode { get; set; }
        public string? SiteType { get; set; }
        public double? AltitudeMin { get; set; }
        public double? AltitudeMax { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public int? N2KVersioningRef { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }
        public Boolean? JustificationRequired { get; set;}
        public Boolean? JustificationProvided { get; set; }
        public DateTime? DateConfSCI { get; set; }
        public Boolean? Priority { get; set; }
        public DateTime? DatePropSCI { get; set; }
        public DateTime? DateSpa { get; set; }
        public DateTime? DateSac { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sites>()
                .ToTable("Sites")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}
