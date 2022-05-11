using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class OwnerType : VersioningBase, IEntityModel
    {

        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string? TYPE { get; set; }
        public decimal? PERCENT { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OwnerType>()
                .ToTable("OWNERTYPE")
                .HasNoKey();
        }

    }
}
