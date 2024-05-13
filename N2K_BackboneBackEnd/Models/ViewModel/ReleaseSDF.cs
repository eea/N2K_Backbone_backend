using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [NotMapped]
    public class ReleaseSDF : IEntityModel
    {
        public SiteInfoRelease SiteInfo { get; set; } = new SiteInfoRelease();
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
    public class SiteInfoRelease
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
        public ReleaseInfo[]? Releases { get; set; }
    }

    [NotMapped]
    public class ReleaseInfo
    {
        public string? ReleaseName { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
