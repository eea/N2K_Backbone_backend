using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteChangeDetailViewModel : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool? HasGeometry { get; set; } = false;
        public int Version { get; set; }
        public LineageTypes? LineageChangeType { get; set; }
        public String? AffectedSites { get; set; }
        public bool? JustificationRequired { get; set; }
        public bool? JustificationProvided { get; set; }
        public SiteChangeStatus? Status { get; set; }
        [NotMapped]
        public SiteChangesLevelDetail Critical { get; set; } = new SiteChangesLevelDetail();
        [NotMapped]
        public SiteChangesLevelDetail Warning { get; set; } = new SiteChangesLevelDetail();
        [NotMapped]
        public SiteChangesLevelDetail Info { get; set; } = new SiteChangesLevelDetail();

        public SiteChangeDetailViewModel()
        {
            this.Critical = new SiteChangesLevelDetail();
            this.Warning = new SiteChangesLevelDetail();
            this.Info = new SiteChangesLevelDetail();
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDetailViewModel>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());
        }
    }
}