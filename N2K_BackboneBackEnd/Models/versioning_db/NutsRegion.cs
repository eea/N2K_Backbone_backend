using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class NutsRegion : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        [Key]
        public int RID { get; set; }
        public string SITECODE { get; set; }
        public string? NUTSCODE { get; set; }
        public decimal? COVER { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsRegion>()
                .ToTable("NUTSREGION")
                .HasNoKey();
        }

    }
}
