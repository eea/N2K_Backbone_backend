using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.releases_db
{
    public class Habitats : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public long ReleaseId { get; set; }
        public string? COUNTRY_CODE { get; set; }
        public string? SITECODE { get; set; }
        public string? HabitatCode { get; set; }
        public string? DESCRIPTION { get; set; }
        public string? HABITAT_PRIORITY { get; set; }
        public bool? PRIORITY_FORM_HABITAT_TYPE { get; set; }
        public int? NON_PRESENCE_IN_SITE { get; set; }
        public decimal? COVER_HA { get; set; }
        public string? CAVES { get; set; }
        public string? REPRESENTATIVITY { get; set; }
        public string? RELSURFACE { get; set; }
        public string? CONSERVATION { get; set; }
        public string? GLOBAL { get; set; }
        public string? DATAQUALITY { get; set; }
        public decimal? PERCENTAGE_COVER { get; set; }
        public bool? INTRODUCTION_CANDIDATE { get; set; }

        private string dbConnection = string.Empty;

        public Habitats() { }

        public Habitats(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("HABITATS")
                .HasKey(c => new { c.ID });
        }
    }

	public class HabitatClass : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? SITECODE { get; set; }
		public string? HABITATCODE { get; set; }
		public decimal? PERCENTAGECOVER { get; set; }
		public string? DESCRIPTION { get; set; }
		
        private string dbConnection = string.Empty;

        public HabitatClass() { }

        public HabitatClass(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatClass>()
                .ToTable("HABITATCLASS")
                .HasKey(c => new { c.ID });
        }
	}
	
	public class Impact : IEntityModel, IEntityModelReleasesDB
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

        public Impact() { }

        public Impact(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Impact>()
                .ToTable("IMPACT")
                .HasKey(c => new { c.ID });
        }
	}

	public class DirectiveSpecies : IEntityModel, IEntityModelReleasesDB
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
		
        private string dbConnection = string.Empty;

        public DirectiveSpecies() { }

        public DirectiveSpecies(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DirectiveSpecies>()
                .ToTable("DIRECTIVESPECIES")
                .HasKey(c => new { c.ID });
        }
	}

	public class DesignationStatus : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? SITECODE { get; set; }
		public string? DESIGNATIONCODE { get; set; }
		public string? DESIGNATEDSITENAME { get; set; }
		public string? OVERLAPCODE { get; set; }
		public string? OVERLAPPERC { get; set; }
		
        private string dbConnection = string.Empty;

        public DesignationStatus() { }

        public DesignationStatus(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DesignationStatus>()
                .ToTable("DESIGNATIONSTATUS")
                .HasKey(c => new { c.ID });
        }
	}

	public class BioRegion : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? SITECODE { get; set; }
		public string? BIOGEOGRAPHICREG { get; set; }
		public float PERCENTAGE { get; set; }
		
        private string dbConnection = string.Empty;

        public BioRegion() { }

        public BioRegion(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegion>()
                .ToTable("BIOREGION")
                .HasKey(c => new { c.ID });
        }
	}

	public class Management : IEntityModel, IEntityModelReleasesDB
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

        public Management() { }

        public Management(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Management>()
                .ToTable("MANAGEMENT")
                .HasKey(c => new { c.ID });
        }
	}

	public class MetaData : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? parameter { get; set; }
		public string? value { get; set; }
		
        private string dbConnection = string.Empty;

        public MetaData() { }

        public MetaData(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MetaData>()
                .ToTable("METADATA")
                .HasKey(c => new { c.ID });
        }
	}

	public class Natura2000Sites : IEntityModel, IEntityModelReleasesDB
	{
		public long ReleaseId { get; set; }
		public string? COUNTRY_CODE { get; set; }
		public string? SITECODE { get; set; }
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
		public string? INSPIRE_ID { get; set; }
		
        private string dbConnection = string.Empty;

        public Natura2000Sites() { }

        public Natura2000Sites(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Natura2000Sites>()
                .ToTable("NATURA2000SITES")
                .HasKey(c => new { c.ReleaseId });
        }
	}

	public class OtherSpecies : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? COUNTRY_CODE { get; set; }
		public string? SITECODE { get; set; }
		public string? SPECIESGROUP { get; set; }
		public string? SPECIESNAME { get; set; }
		public string? SPECIESCODE { get; set; }
		public string? MOTIVATION { get; set; }
		public bool SENSITIVE { get; set; }
		public bool NONPRESENCEINSITE { get; set; }
		public int LOWERBOUND { get; set; }
		public int UPPERBOUND { get; set; }
		public string? ABUNDANCE_CATEGORY { get; set; }
		public string? COUNTING_UNIT { get; set; }
		public bool INTRODUCTION_CANDIDATE { get; set; }
		
        private string dbConnection = string.Empty;

        public OtherSpecies() { }

        public OtherSpecies(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OtherSpecies>()
                .ToTable("OTHERSPECIES")
                .HasKey(c => new { c.ID });
        }
	}

	public class Releases : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public string? Title { get; set; }
		public string? Author { get; set; }
		public DateTime? CreateDate { get; set; }
		public bool Final { get; set; }
		public string? ModifyUser { get; set; }
		public DateTime? ModifyDate { get; set; }
		public string? Character { get; set; }
		
        private string dbConnection = string.Empty;

        public Releases() { }

        public Releases(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Releases>()
                .ToTable("RELEASES")
                .HasKey(c => new { c.ID });
        }
	}

	public class Species : IEntityModel, IEntityModelReleasesDB
	{
		public long ID { get; set; }
		public long ReleaseId { get; set; }
		public string? COUNTRY_CODE { get; set; }
		public string? SITECODE { get; set; }
		public string? SPECIESNAME { get; set; }
		public string? SPECIESCODE { get; set; }
		public string? REF_SPGROUP { get; set; }
		public string? SPGROUP { get; set; }
		public bool SENSITIVE { get; set; }
		public bool NONPRESENCEINSITE { get; set; }
		public string? POPULATION_TYPE { get; set; }
		public int LOWERBOUND { get; set; }
		public int UPPERBOUND { get; set; }
		public string? COUNTING_UNIT { get; set; }
		public string? ABUNDANCE_CATEGORY { get; set; }
		public string? DATAQUALITY { get; set; }
		public string? POPULATION { get; set; }
		public string? CONSERVATION { get; set; }
		public string? ISOLATION { get; set; }
		public string? GLOBAL { get; set; }
		public bool INTRODUCTION_CANDIDATE { get; set; }
		
        private string dbConnection = string.Empty;

        public Species() { }

        public Species(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Species>()
                .ToTable("SPECIES")
                .HasKey(c => new { c.ID });
        }
	}

	public class SiteSpatial : IEntityModel, IEntityModelReleasesDB
	{
		public long ReleaseId { get; set; }
		public string? siteCode { get; set; }
		public string? data { get; set; }
		
        private string dbConnection = string.Empty;

        public SiteSpatial() { }

        public SiteSpatial(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteSpatial>()
                .ToTable("SITESPATIAL")
                .HasKey(c => new { c.ReleaseId });
        }
	}
}
