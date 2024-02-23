using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class StatusChangesRelease : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; set; }
        public string CountryCode { get; set; } = String.Empty;
        public long? Release { get; set; }
        public DateTime? Date { get; set; }
        public string? Owner { get; set; }
        public string? Comments { get; set; }
        public int? Edited { get; set; }
        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<StatusChangesRelease>()
                .ToTable("StatusChangesRelease");
        }
    }
}
