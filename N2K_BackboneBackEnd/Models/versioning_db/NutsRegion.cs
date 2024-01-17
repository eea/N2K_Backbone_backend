using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class NutsRegion : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; } = "";
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public int RID { get; set; }
        public string SITECODE { get; set; } = "";
        public string NUTSCODE { get; set; } = "";
        [Column(TypeName = "decimal(38, 2)")]
        public decimal? COVER { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsRegion>()
                .ToTable("NUTSREGION")
                .HasNoKey();
        }
    }
}