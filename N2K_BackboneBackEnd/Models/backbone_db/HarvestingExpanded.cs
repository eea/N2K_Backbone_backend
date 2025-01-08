using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class HarvestingExpanded : IEntityModel
    {
        [Key]
        public long Id { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? Country { get; set; }
        public string? Name { get; set; }
        public HarvestingStatus Status { get; set; } = HarvestingStatus.Pending;
        public int Version { get; set; }
        [NotMapped]
        public int? DataLoaded { get; set; }
        public string? CDR { get; set; }
        public string? CDRLink { get; set; }
        public int ChangesTotal { get; set; }
        public int ChangesAccepted { get; set; }
        public int ChangesPending { get; set; }
        public int ChangesRejected { get; set; }
        public int SitesTotal { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HarvestingExpanded>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.HarvestingStatus>());
        }
    }
}
