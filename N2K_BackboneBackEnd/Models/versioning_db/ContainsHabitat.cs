using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class ContainsHabitat : VersioningBase, IEntityModel
    {
        public int RID { get; set; }
        public string COUNTRYCODE { get; set; } = "";

        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; } = "";
        public string HABITATCODE { get; set; } = "";

        [Column(TypeName = "decimal(38, 2)")]
        public decimal? PERCENTAGECOVER { get; set; }
        public string? REPRESENTATIVITY { get; set; }
        public string? RELSURFACE { get; set; }
        public string? CONSSTATUS { get; set; }
        public string? GLOBALASSESMENT { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public Int16? NONPRESENCEINSITE { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CAVES { get; set; }
        public string? DATAQUALITY { get; set; }
        public double? COVER_HA { get; set; }
        public bool? PF { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ContainsHabitat>()
                .ToTable("CONTAINSHABITAT")
                .HasNoKey(); 
        }

    }
}
