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
            builder.Entity<ReleaseSDF>().HasNoKey();
        }
    }

    [NotMapped]
    public class SiteInfoRelease : SiteInfo
    {
        public List<ReleaseInfo>? Releases { get; set; } = new List<ReleaseInfo>();
    }

    [NotMapped]
    public class ReleaseInfo
    {
        public long? ReleaseId { get; set; }
        public string? ReleaseName { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
