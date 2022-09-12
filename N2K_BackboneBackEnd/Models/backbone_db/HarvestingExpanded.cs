using System.ComponentModel.DataAnnotations;
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
        public string Country { get; set; }

        public string Name { get; set; }

        public HarvestingStatus Status { get; set; } = HarvestingStatus.Pending;
        public int Version { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HarvestingExpanded>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.HarvestingStatus>());
        }


    }
}
