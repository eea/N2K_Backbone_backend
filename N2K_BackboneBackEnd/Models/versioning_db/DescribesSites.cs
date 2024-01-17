using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class DescribesSites : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; } = "";
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; } = "";
        public string HABITATCODE { get; set; } = "";
        [Column(TypeName = "decimal(38, 2)")]
        public decimal? PERCENTAGECOVER { get; set; }
        public int? RID { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DescribesSites>()
                .ToTable("DESCRIBESSITES")
                .HasNoKey();
        }
    }
}