using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [NotMapped]
    public class SDF : IEntityModel
    {
        public SiteInfo SiteInfo { get; set; } = new SiteInfo();
        public SiteIdentification SiteIdentification { get; set; } = new SiteIdentification();
        public SiteLocation SiteLocation { get; set; } = new SiteLocation();
        public EcologicalInformation EcologicalInformation { get; set; } = new EcologicalInformation();
        public SiteDescription SiteDescription { get; set; } = new SiteDescription();
        public SiteProtectionStatus SiteProtectionStatus { get; set; } = new SiteProtectionStatus();
        public SiteManagement SiteManagement { get; set; } = new SiteManagement();
        public MapOfTheSite MapOfTheSite { get; set; } = new MapOfTheSite();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SDF>().HasNoKey();
        }
    }

    [NotMapped]
    public class SiteInfo
    {
        public string? SiteName { get; set; }
        public string? Country { get; set; }
        public string? Directive { get; set; }
        public string? SiteCode { get; set; }
        public decimal? Area { get; set; }
        public DateTime? Est { get; set; }
        public decimal? MarineArea { get; set; }
        public int? Habitats { get; set; }
        public int? Species { get; set; }
    }

    [NotMapped]
    public class SiteIdentification
    {
        public string? Type { get; set; }
        public string? SiteCode { get; set; }
        public string? SiteName { get; set; }
        public DateTime? FirstCompletionDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public Respondent Respondent { get; set; } = new Respondent(); 
        public List<SiteDesignation> SiteDesignation { get; set; } = new List<SiteDesignation>();
    }

    [NotMapped]
    public class Respondent
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    [NotMapped]
    public class SiteDesignation
    {
        public DateTime? ClassifiedSPA { get; set; }
        public string? ReferenceSPA { get; set; }
        public DateTime? ProposedSCI { get; set; }
        public DateTime? ConfirmedSCI { get; set; }
        public DateTime? DesignatedSAC { get; set; }
        public string? ReferenceSAC { get; set; }
        public string? Explanations { get; set; }
    }

    [NotMapped]
    public class SiteLocation
    {
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Area { get; set; }
        public decimal? MarineArea { get; set; }
        public decimal? SiteLength { get; set; }
        public List<Region> Region { get; set; } = new List<Region>();
        public List<BiogeographicalRegions> BiogeographicalRegions { get; set; } = new List<BiogeographicalRegions>();
    }

    [NotMapped]
    public class BiogeographicalRegions
    {
        public string? Name { get; set; }
        public double? Value { get; set; }
    }

    [NotMapped]
    public class Region
    {
        public string? NUTSLevel2Code { get; set; }
        public string? RegionName { get; set; }
    }

    [NotMapped]
    public class EcologicalInformation
    {
        public List<HabitatSDF> HabitatTypes { get; set; } = new List<HabitatSDF>();
        public List<SpeciesSDF> Species { get; set; } = new List<SpeciesSDF>();
        public List<SpeciesSDF> OtherSpecies { get; set; } = new List<SpeciesSDF>();
    }

    [NotMapped]
    public class HabitatSDF
    {
        public string? HabitatName { get; set; }
        public string? Code { get; set; }
        public string? PF { get; set; }
        public string? NP { get; set; }
        public decimal? Cover { get; set; }
        public string? Cave { get; set; }
        public string? DataQuality { get; set; }
        public string? Representativity { get; set; }
        public string? RelativeSurface { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
    }

    [NotMapped]
    public class SpeciesSDF
    {
        public string? SpeciesName { get; set; }
        public string? Code { get; set; }
        public string? Group { get; set; }
        public string? Sensitive { get; set; }
        public string? NP { get; set; }
        public string? Type { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string? Unit { get; set; }
        public string? Category { get; set; }
        public string? DataQuality { get; set; }
        public string? Population { get; set; }
        public string? Conservation { get; set; }
        public string? Isolation { get; set; }
        public string? Global { get; set; }
    }

    [NotMapped]
    public class SiteDescription
    {
        public List<CodeCover> GeneralCharacter { get; set; } = new List<CodeCover>();
        public string? Quality { get; set; }
        public List<Threats> NegativeThreats { get; set; } = new List<Threats>();
        public List<Threats> PositiveThreats { get; set; } = new List<Threats>();
        public List<Ownership> Ownership { get; set; } = new List<Ownership>();
        public string? Documents { get; set; }
        public List<string> Links { get; set; } = new List<string>();
    }

    [NotMapped]
    public class CodeCover
    {
        public string? Code { get; set; }
        public decimal? Cover { get; set; }
    }

    [NotMapped]
    public class Threats
    {
        public string? Rank { get; set; }
        public string? Impacts { get; set; }
        public string? Pollution { get; set; }
        public string? Origin { get; set; }
    }

    [NotMapped]
    public class Ownership
    {
        public string? Type { get; set; }
        public decimal? Percent { get; set; }
    }

    [NotMapped]
    public class SiteProtectionStatus
    {
        public List<CodeCover> DesignationTypes { get; set; } = new List<CodeCover>();
        public List<RelationSites> RelationSites { get; set; } = new List<RelationSites>();
        public string? SiteDesignation { get; set; }
    }

    [NotMapped]
    public class RelationSites
    {
        public string? DesignationLevel { get; set; }
        public string? TypeCode { get; set; }
        public string? SiteName { get; set; }
        public string? Type { get; set; }
        public decimal? Percent { get; set; }
    }

    [NotMapped]
    public class SiteManagement
    {
        public List<BodyResponsible> BodyResponsible { get; set; } = new List<BodyResponsible>();
        public List<ManagementPlan> ManagementPlan { get; set; } = new List<ManagementPlan>();
        public string? ConservationMeasures { get; set; }
    }

    [NotMapped]
    public class BodyResponsible
    {
        public string? Organisation { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    [NotMapped]
    public class ManagementPlan
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
    }

    [NotMapped]
    public class MapOfTheSite
    {
        public string? INSPIRE { get; set; }
        public string? MapDelivered { get; set; }
    }
}
