using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class IsImpactedBy : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; } = "";
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; } = "";
        public int RID { get; set; }
        public string? ACTIVITYCODE { get; set; }
        public string? IN_OUT { get; set; }
        public string? INTENSITY { get; set; }
        public float? PERCENTAGEAFF { get; set; }
        public string? INFLUENCE { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public string? POLLUTIONCODE { get; set; }
        public string? OCCURRENCE { get; set; }
        public string? IMPACTTYPE { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .ToTable("ISIMPACTEDBY")
                .HasNoKey();
        }
    }
}