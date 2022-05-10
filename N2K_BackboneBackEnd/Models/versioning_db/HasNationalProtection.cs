using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class HasNationalProtection : IEntityModel
    {

        public string COUNTRYCODE { get; set; }
        public int VERSIONID { get; set; }
        public int COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public int RID { get; set; }
        public string? DESIGNATEDCODE { get; set; }
        public float? PERCENTAGE { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HasNationalProtection>()
                .ToTable("HASNATIONALPROTECTION")
                .HasNoKey();
        }

    }

    //    
}
