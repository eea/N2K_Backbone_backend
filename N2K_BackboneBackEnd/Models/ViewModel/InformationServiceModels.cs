using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class ReleasesCatalog : IEntityModel
    {
        [Key]
        public long? ReleaseId { get; set; }
        public string? ReleaseName { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleasesCatalog>();
        }
    }

    [Keyless]
    public class ReleaseCounters : IEntityModel
    {
        public int? SitesNumber { get; set; }
        public int? HabitatsNumber { get; set; }
        public int? SpeciesNumber { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleaseCounters>();
        }
    }

    [Keyless]
    public class HabitatsParametered : IEntityModel
    {
        public string? HabitatCode { get; set; }
        public string? HabitatName { get; set; }
        public string? HabitatImageUrl { get; set; }
        public int? SitesNumber { get; set; } //number of sites in which the habitat appears

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatsParametered>();
        }
    }

    [Keyless]
    public class HabitatsParameteredExtended : HabitatsParametered, IEntityModel
    {
        public long? ReleaseId { get; set; }
        public string? Country { get; set; }
        public string? BioRegion { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatsParameteredExtended>();
        }
    }

    [Keyless]
    public class SitesParametered : IEntityModel
    {
        public string? SiteCode { get; set; }
        public string? SiteName { get; set; }
        public string? SiteTypeCode { get; set; }
        public decimal? SiteArea { get; set; }
        public int? HabitatsNumber { get; set; } //number of habitats that appear on the site
        public int? SpeciesNumber { get; set; } //number of species that appear on the site
        public Boolean? IsSentitive { get; set; } //returns true if the site contains sensitive species

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SitesParametered>();
        }
    }

    [Keyless]
    public class SitesParameteredExtended : SitesParametered, IEntityModel
    {
        public long? ReleaseId { get; set; }
        public string? BioRegion { get; set; }
        public string? HABITATCODE { get; set; }
        public string? SPECIESCODE { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SitesParameteredExtended>();
        }
    }

    [Keyless]
    public class SpeciesParametered : IEntityModel
    {
        public string? SpeciesCode { get; set; }
        public string? SpeciesName { get; set; }
        public string? SpeciesScientificName { get; set; }
        public string? SpeciesGroupCode { get; set; }
        public string? SpeciesEunisId { get; set; }
        public string? SpeciesImageUrl { get; set; }
        public int? SitesNumber { get; set; } //number of sites in which the habitat appears

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesParametered>();
        }
    }

    [Keyless]
    public class SpeciesParameteredExtended : SpeciesParametered, IEntityModel
    {
        public long? ReleaseId { get; set; }
        public string? Country { get; set; }
        public string? BioRegion { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesParameteredExtended>();
        }
    }

    #region Unused
    /*
    public class BioregionCatalog : IEntityModel
    {
        public string? BioregionName { get; set; }
        public string? BioregionCode { get; set; }
        public Boolean? IsMarine { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioregionCatalog>();
        }
    }

    public class CountriesCatalog : IEntityModel
    {
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountriesCatalog>();
        }
    }

    public class HabitatGroupsCatalog : IEntityModel
    {
        public string? HabitatGroupName { get; set; }
        public string? HabitatGroupCode { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatGroupsCatalog>();
        }
    }

    public class SiteTypesCatalog : IEntityModel
    {
        public string? SiteTypeName { get; set; }
        public string? SiteTypeCode { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteTypesCatalog>();
        }
    }

    public class SpeciesGroupsCatalog : IEntityModel
    {
        public string? SpeciesGroupName { get; set; }
        public string? SpeciesGroupCode { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesGroupsCatalog>();
        }
    }
    */
    #endregion
}