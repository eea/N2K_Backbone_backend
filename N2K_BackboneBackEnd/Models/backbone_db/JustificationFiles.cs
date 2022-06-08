using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class JustificationFiles : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public int Id { get; set; }

        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }

        public String? Path { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<JustificationFiles>()
                .ToTable("JustificationFiles");

        }
    }
}
