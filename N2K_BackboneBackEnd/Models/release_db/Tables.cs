using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.releases_db
{
    public class Habitats : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long ReleaseId { get; set; }
        public string? CountryCode { get; set; }
        public string? SiteCode { get; set; }
        public string? HabitatCode { get; set; }
        public string? Description { get; set; }
        public string? HabitatPriority { get; set; }
        public bool? PriorityFormHabitatType { get; set; }
        public int? NonPresenceInSite { get; set; }
        public decimal? CoverHa { get; set; }
        public string? Caves { get; set; }
        public string? Representativity { get; set; }
        public string? RelSurface { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public string? DataQuality { get; set; }
        public decimal? PercentageCover { get; set; }
        public bool? IntroductionCandidate { get; set; }

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
                .HasKey(c => new { c.id });
        }
    }

	public class HabitatClass : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? HabitatCode { get; set; }
		public decimal? PercentageCover { get; set; }
		public string? Description { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}
	
	public class Impact : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? ImpactCode { get; set; }
		public string? Description { get; set; }
		public string? Intensity { get; set; }
		public string? PollutionCode { get; set; }
		public string? Occurrence { get; set; }
		public string? ImpactType { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class DirectiveSpecies : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? Directive { get; set; }
		public string? SpeciesName { get; set; }
		public string? AnnexII { get; set; }
		public string? AnnexII1 { get; set; }
		public string? AnnexII2 { get; set; }
		public string? AnnexIII1 { get; set; }
		public string? AnnexIII2 { get; set; }
		public string? AnnexIV { get; set; }
		public string? AnnexV { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class DesignationStatus : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? DesignationCode { get; set; }
		public string? DesignatedSiteName { get; set; }
		public string? OverlapCode { get; set; }
		public string? OverlapPerc { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class BioRegion : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? BioGeoGraphicReg { get; set; }
		public float Percentage { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class Management : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? OrgName { get; set; }
		public string? OrgEmail { get; set; }
		public string? ManagConservMeasures { get; set; }
		public string? ManagPlan { get; set; }
		public string? ManagPlanUrl { get; set; }
		public string? ManagStatus { get; set; }
		public string? OrgLocatorName { get; set; }
		public string? OrgDesignator { get; set; }
		public string? OrgAdminUnit { get; set; }
		public string? OrgPostCode { get; set; }
		public string? OrgPostName { get; set; }
		public string? OrgAddress { get; set; }
		public string? OrgAddressUnstructured { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class MetaData : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
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
                .HasKey(c => new { c.id });
        }
	}

	public class Natura2000Sites : IEntityModel, IEntityModelReleasesDB
	{
		public long ReleaseId { get; set; }
		public string? CountryCode { get; set; }
		public string? SiteCode { get; set; }
		public int Version { get; set; }
		public string? SiteName { get; set; }
		public string? SiteType { get; set; }
		public DateTime? DateCompilation { get; set; }
		public DateTime? DateUpdate { get; set; }
		public DateTime? DateSpa { get; set; }
		public string? SpaLegalReference { get; set; }
		public DateTime? DatePropSci { get; set; }
		public DateTime? DateConfSci { get; set; }
		public DateTime? DateSac { get; set; }
		public string? SacLegalReference { get; set; }
		public string? Explanations { get; set; }
		public decimal Areaha { get; set; }
		public decimal Lengthkm { get; set; }
		public decimal MarineAreaPercentage { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		public string? Documentation { get; set; }
		public string? Quality { get; set; }
		public string? Designation { get; set; }
		public string? Othercharact { get; set; }
		public string? InspireId { get; set; }
		
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
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? CountryCode { get; set; }
		public string? SiteCode { get; set; }
		public string? SpeciesGroup { get; set; }
		public string? SpeciesName { get; set; }
		public string? SpeciesCode { get; set; }
		public string? Motivation { get; set; }
		public bool Sensitive { get; set; }
		public bool NonPresenceInSite { get; set; }
		public int Lowerbound { get; set; }
		public int Upperbound { get; set; }
		public string? AbundanceCategory { get; set; }
		public string? CountingUnit { get; set; }
		public bool IntroductionCandidate { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class Releases : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
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
                .HasKey(c => new { c.id });
        }
	}

	public class Species : IEntityModel, IEntityModelReleasesDB
	{
		public long id { get; set; }
		public long ReleaseId { get; set; }
		public string? CountryCode { get; set; }
		public string? SiteCode { get; set; }
		public string? SpeciesName { get; set; }
		public string? SpeciesCode { get; set; }
		public string? RefSpgroup { get; set; }
		public string? SPgroup { get; set; }
		public bool Sensitive { get; set; }
		public bool NonPresenceInSite { get; set; }
		public string? PopulationType { get; set; }
		public int Lowerbound { get; set; }
		public int Upperbound { get; set; }
		public string? CountingUnit { get; set; }
		public string? AbundanceCategory { get; set; }
		public string? DataQuality { get; set; }
		public string? Population { get; set; }
		public string? Conservation { get; set; }
		public string? Isolation { get; set; }
		public string? Global { get; set; }
		public bool IntroductionCandidate { get; set; }
		
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
                .HasKey(c => new { c.id });
        }
	}

	public class SiteSpatial : IEntityModel, IEntityModelReleasesDB
	{
		public long ReleaseId { get; set; }
		public string? SiteCode { get; set; }
		public string? Data { get; set; }
		
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
