using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class SiteChangesSummary : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; }
        public int Version { get; set; }
        public int NumCritical { get; set; }
        public int NumWarning { get; set; }
        public int NumInfo { get; set; }
        public string Name { get; set; }
        public string SiteType { get; set; }

        [NotMapped]
        public string CountryCode { get; set; } = string.Empty;
        public SiteChangeStatus? Status { get; set; }

        public string? Author { get; set; }
        public DateTime? Date { get; set; }
        public LineageTypes? LineageType { get; set; }

        public bool? JustificationRequired { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangesSummary>()
                .HasNoKey()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());
            
            builder.Entity<SiteChangesSummary>()
                .HasNoKey()
                .Property(e => e.LineageType)
                .HasConversion(new EnumToStringConverter<Enumerations.LineageTypes>());
            
        }
    }
}
