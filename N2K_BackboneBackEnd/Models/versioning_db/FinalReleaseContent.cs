using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class FinalReleaseContent : VersioningBase, IEntityModel
    {
        public float IDFinalRelease { get; set; }
        public string CountryCode { get; set; }
        public float CountryVersionID { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<FinalReleaseContent>()
                .ToTable("FinalReleaseContent")
                .HasNoKey();
        }
    }
}
