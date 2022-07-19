using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class PackageCountry : VersioningBase, IEntityModel
    {
        public string CountryCode { get; set; }
        public decimal CountryVersionID { get; set; }
        public string? Path { get; set; }
        public DateTime? Importdate { get; set; }
        public decimal VERSIONID { get; set; }
        public bool Versioned { get; set; }
        public bool Discarded { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PackageCountry>()
                .ToTable("PackageCountry")
                .HasKey(c=> new { c.CountryCode, c.CountryVersionID });
        }
    }


    [Keyless]
    public class PackageCountrySpatial : VersioningBase, IEntityModel
    {
        public string CountryCode { get; set; }
        public decimal CountryVersionID { get; set; }
        public string? Path { get; set; }
        public DateTime? Importdate { get; set; }
        public decimal VERSIONID { get; set; }
        public bool Versioned { get; set; }
        public bool Discarded { get; set; }
        public string SpatialRef { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PackageCountrySpatial>()
                .ToTable("PackageCountrySpatial")
                .HasKey(c => new { c.CountryCode, c.CountryVersionID });
        }
    }

}
