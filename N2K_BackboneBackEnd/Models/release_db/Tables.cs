using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.release_db
{
    public class BIOREGION : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? BIOGEOGRAPHICREG { get; set; }
        public double? PERCENTAGE { get; set; }

        private string dbConnection = string.Empty;

        public BIOREGION() { }

        public BIOREGION(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BIOREGION>()
                .ToTable("BIOREGION")
                .HasKey(c => new { c.ID });
        }
    }

    public class DESIGNATIONSTATUS : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? DESIGNATIONCODE { get; set; }
        public string? DESIGNATEDSITENAME { get; set; }
        public string? OVERLAPCODE { get; set; }
        public string? OVERLAPPERC { get; set; }

        private string dbConnection = string.Empty;

        public DESIGNATIONSTATUS() { }

        public DESIGNATIONSTATUS(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DESIGNATIONSTATUS>()
                .ToTable("DESIGNATIONSTATUS")
                .HasKey(c => new { c.ID });
        }
    }

    public class DIRECTIVESPECIES : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? DIRECTIVE { get; set; }
        public string? SPECIESNAME { get; set; }
        public string? ANNEXII { get; set; }
        public string? ANNEXII1 { get; set; }
        public string? ANNEXII2 { get; set; }
        public string? ANNEXIII1 { get; set; }
        public string? ANNEXIII2 { get; set; }
        public string? ANNEXIV { get; set; }
        public string? ANNEXV { get; set; }
        public string? SPBCAX1 { get; set; }

        private string dbConnection = string.Empty;

        public DIRECTIVESPECIES() { }

        public DIRECTIVESPECIES(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DIRECTIVESPECIES>()
                .ToTable("DIRECTIVESPECIES")
                .HasKey(c => new { c.ID });
        }
    }

    public class HABITATCLASS : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? HABITATCODE { get; set; }
        public decimal? PERCENTAGECOVER { get; set; }
        public string? DESCRIPTION { get; set; }

        private string dbConnection = string.Empty;

        public HABITATCLASS() { }

        public HABITATCLASS(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HABITATCLASS>()
                .ToTable("HABITATCLASS")
                .HasKey(c => new { c.ID });
        }
    }

    public class HABITATS : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? HABITATCODE { get; set; }
        public string? DESCRIPTION { get; set; }
        public string? HABITAT_PRIORITY { get; set; }
        public bool? PRIORITY_FORM_HABITAT_TYPE { get; set; }
        public int? NON_PRESENCE_IN_SITE { get; set; }
        public decimal? COVER_HA { get; set; }
        public string? CAVES { get; set; }
        public string? REPRESENTATIVITY { get; set; }
        public string? RELSURFACE { get; set; }
        public string? CONSERVATION { get; set; }
        public string? GLOBAL_ASSESSMENT { get; set; }
        public string? DATAQUALITY { get; set; }
        public decimal? PERCENTAGE_COVER { get; set; }
        public bool? INTRODUCTION_CANDIDATE { get; set; }

        private string dbConnection = string.Empty;

        public HABITATS() { }

        public HABITATS(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HABITATS>()
                .ToTable("HABITATS")
                .HasKey(c => new { c.ID });
        }
    }

    public class IMPACT : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? IMPACTCODE { get; set; }
        public string? DESCRIPTION { get; set; }
        public string? INTENSITY { get; set; }
        public string? POLLUTIONCODE { get; set; }
        public string? OCCURRENCE { get; set; }
        public string? IMPACT_TYPE { get; set; }

        private string dbConnection = string.Empty;

        public IMPACT() { }

        public IMPACT(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IMPACT>()
                .ToTable("IMPACT")
                .HasKey(c => new { c.ID });
        }
    }

    public class MANAGEMENT : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? SITECODE { get; set; }
        public string? ORG_NAME { get; set; }
        public string? ORG_EMAIL { get; set; }
        public string? MANAG_CONSERV_MEASURES { get; set; }
        public string? MANAG_PLAN { get; set; }
        public string? MANAG_PLAN_URL { get; set; }
        public string? MANAG_STATUS { get; set; }
        public string? ORG_LOCATORNAME { get; set; }
        public string? ORG_DESIGNATOR { get; set; }
        public string? ORG_ADMINUNIT { get; set; }
        public string? ORG_POSTCODE { get; set; }
        public string? ORG_POSTNAME { get; set; }
        public string? ORG_ADDRESS { get; set; }
        public string? ORG_ADDRESS_UNSTRUCTURED { get; set; }

        private string dbConnection = string.Empty;

        public MANAGEMENT() { }

        public MANAGEMENT(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MANAGEMENT>()
                .ToTable("MANAGEMENT")
                .HasKey(c => new { c.ID });
        }
    }

    public class METADATA : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? parameter { get; set; }
        public string? value { get; set; }

        private string dbConnection = string.Empty;

        public METADATA() { }

        public METADATA(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<METADATA>()
                .ToTable("METADATA")
                .HasKey(c => new { c.ID });
        }
    }

    public class NATURA2000SITES : IEntityModel, IEntityModelReleasesDB
    {
        public long ReleaseId { get; set; }
        public string COUNTRY_CODE { get; set; }
        public string SITECODE { get; set; }
        public int VERSION { get; set; }
        public string? SITENAME { get; set; }
        public string? SITETYPE { get; set; }
        public DateTime? DATE_COMPILATION { get; set; }
        public DateTime? DATE_UPDATE { get; set; }
        public DateTime? DATE_SPA { get; set; }
        public string? SPA_LEGAL_REFERENCE { get; set; }
        public DateTime? DATE_PROP_SCI { get; set; }
        public DateTime? DATE_CONF_SCI { get; set; }
        public DateTime? DATE_SAC { get; set; }
        public string? SAC_LEGAL_REFERENCE { get; set; }
        public string? EXPLANATIONS { get; set; }
        public decimal AREAHA { get; set; }
        public decimal LENGTHKM { get; set; }
        public decimal MARINE_AREA_PERCENTAGE { get; set; }
        public decimal LONGITUDE { get; set; }
        public decimal LATITUDE { get; set; }
        public string? DOCUMENTATION { get; set; }
        public string? QUALITY { get; set; }
        public string? DESIGNATION { get; set; }
        public string? OTHERCHARACT { get; set; }
        public DateTime? RELEASE_DATE { get; set; }

        private string dbConnection = string.Empty;

        public NATURA2000SITES() { }

        public NATURA2000SITES(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NATURA2000SITES>()
                .ToTable("NATURA2000SITES")
                .HasKey(c => new { c.ReleaseId, c.SITECODE });
        }
    }

    public class OTHERSPECIES : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? COUNTRY_CODE { get; set; }
        public string? SITECODE { get; set; }
        public string? SPECIESGROUP { get; set; }
        public string? SPECIESNAME { get; set; }
        public string? SPECIESCODE { get; set; }
        public string? MOTIVATION { get; set; }
        public bool? SENSITIVE { get; set; }
        public bool? NONPRESENCEINSITE { get; set; }
        public int? LOWERBOUND { get; set; }
        public int? UPPERBOUND { get; set; }
        public string? ABUNDANCE_CATEGORY { get; set; }
        public string? COUNTING_UNIT { get; set; }
        public bool? INTRODUCTION_CANDIDATE { get; set; }

        private string dbConnection = string.Empty;

        public OTHERSPECIES() { }

        public OTHERSPECIES(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OTHERSPECIES>()
                .ToTable("OTHERSPECIES")
                .HasKey(c => new { c.ID });
        }
    }

    public class SiteSpatial : IEntityModel, IEntityModelReleasesDB
    {
        public long ReleaseId { get; set; }
        public string? siteCode { get; set; }
        public string? data { get; set; }
        public decimal? area_ha { get; set; }

        private string dbConnection = string.Empty;

        public SiteSpatial() { }

        public SiteSpatial(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteSpatial>()
                .ToTable("SiteSpatial")
                .HasKey(c => new { c.ReleaseId, c.siteCode });
        }
    }

    public class SPECIES : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? COUNTRY_CODE { get; set; }
        public string? SITECODE { get; set; }
        public string? SPECIESNAME { get; set; }
        public string? SPECIESCODE { get; set; }
        public string? REF_SPGROUP { get; set; }
        public string? SPGROUP { get; set; }
        public bool? SENSITIVE { get; set; }
        public bool? NONPRESENCEINSITE { get; set; }
        public string? POPULATION_TYPE { get; set; }
        public int? LOWERBOUND { get; set; }
        public int? UPPERBOUND { get; set; }
        public string? COUNTING_UNIT { get; set; }
        public string? ABUNDANCE_CATEGORY { get; set; }
        public string? DATAQUALITY { get; set; }
        public string? POPULATION { get; set; }
        public string? CONSERVATION { get; set; }
        public string? ISOLATION { get; set; }
        public string? GLOBAL { get; set; }
        public bool? INTRODUCTION_CANDIDATE { get; set; }

        private string dbConnection = string.Empty;

        public SPECIES() { }

        public SPECIES(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SPECIES>()
                .ToTable("SPECIES")
                .HasKey(c => new { c.ID });
        }
    }
}
