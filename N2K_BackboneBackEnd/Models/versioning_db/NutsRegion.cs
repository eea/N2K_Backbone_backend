using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class NutsRegion : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; }
        public int VERSIONID { get; set; }
        public int COUNTRYVERSIONID { get; set; }
        [Key]
        public int RID { get; set; }
        public string SITECODE { get; set; }
        public string? NUTSCODE { get; set; }
        public float? COVER { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsRegion>()
                .ToTable("NUTSREGION");
        }

    }
}
