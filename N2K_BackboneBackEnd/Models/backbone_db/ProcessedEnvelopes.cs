using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class ProcessedEnvelopes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; }

        public DateTime ImportDate { get; set; } = DateTime.MinValue;
        public string? Country { get;  set; }

        public int Version { get;   set; }

        public string? Importer { get; set; }

        public HarvestingStatus Status { get;  set; }

        public DateTime N2K_VersioningDate { get; set; } = DateTime.MinValue;


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessedEnvelopes>()
                .ToTable("ProcessedEnvelopes")
                .HasKey("Id");

        }
       
    }
}
