using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class Documentationlink : IEntityModel
    {

        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public int RID { get; set; }
        public string? URL { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Documentationlink>()
                .ToTable("DOCUMENTATION_LINK")
                .HasNoKey();
        }

    }

    //    
}
