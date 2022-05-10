using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class PackageCountry : VersioningBase, IEntityModel
    {
        public string CountryCode { get; set; }
        public int CountryVersionID { get; set; }
        public string? Path { get; set; }
        public DateTime? Importdate { get; set; }
        public float VERSIONID { get; set; }
        public bool Versioned { get; set; }
        public bool Discarded { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PackageCountry>()
                .ToTable("PackageCountry")
                .HasKey(c=> new { c.CountryCode, c.CountryVersionID });
        }
    }
}
