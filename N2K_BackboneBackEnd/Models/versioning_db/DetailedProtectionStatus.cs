using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class DetailedProtectionStatus : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string N2K_SITECODE { get; set; }
        public int RID { get; set; }
        public string? DESIGNATIONCODE { get; set; }
        public string? PROTECTEDSITENAME { get; set; }
        public string? OVERLAPCODE { get; set; }

        [Column(TypeName = "decimal(38, 2)")]
        public decimal? OVERLAPPERC { get; set; }
        public string? CONVENTION { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DetailedProtectionStatus>()
                .ToTable("DETAILEDPROTECTIONSTATUS")
                .HasNoKey();
        }

    }
}
