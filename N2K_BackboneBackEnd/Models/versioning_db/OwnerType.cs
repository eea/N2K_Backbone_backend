using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class OwnerType : VersioningBase, IEntityModel
    {

        public string COUNTRYCODE { get; set; }
        public int VERSIONID { get; set; }
        public int COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string? TYPE { get; set; }
        public float? PERCENT { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OwnerType>()
                .ToTable("OWNERTYPE");
        }

    }
}
