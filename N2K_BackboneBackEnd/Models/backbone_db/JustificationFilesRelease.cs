using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class JustificationFilesRelease : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ID { get; set; }
        public string CountryCode { get; set; } = String.Empty;
        public long? Release { get; set; }
        public String Path { get; set; } = String.Empty;
        public String OriginalName { get; set; } = String.Empty;
        public DateTime? ImportDate { get; set; }
        public string? Username { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<JustificationFilesRelease>()
                .ToTable("JustificationFilesRelease");
        }
    }
}
