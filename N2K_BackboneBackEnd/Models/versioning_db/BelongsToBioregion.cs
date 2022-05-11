using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class BelongsToBioRegion : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public int BIOREGID { get; set; }
        public DateTime? PUBL_DATE { get; set; }
        public decimal? PERCENTAGE { get; set; }
       
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BelongsToBioRegion>()
                .HasNoKey()
                .ToTable("BELONGSTOBIOREGION");
        }
    }
}
