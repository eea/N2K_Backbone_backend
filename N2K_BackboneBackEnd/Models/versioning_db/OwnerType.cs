using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class OwnerType : VersioningBase, IEntityModel
    {

        public string COUNTRYCODE { get; set; } = "";
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; } = "";
        public string? TYPE { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? PERCENT { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OwnerType>()
                .ToTable("OWNERTYPE")
                .HasNoKey();
        }

    }
}
