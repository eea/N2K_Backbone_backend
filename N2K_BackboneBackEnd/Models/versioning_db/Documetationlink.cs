using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class Documetationlink : IEntityModel
    {

        public string COUNTRYCODE { get; set; }
        public int VERSIONID { get; set; }
        public int COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public int RID { get; set; }
        public string? URL { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Documetationlink>()
                .ToTable("DOCUMENTATION_LINK");
        }

    }

    //    
}
