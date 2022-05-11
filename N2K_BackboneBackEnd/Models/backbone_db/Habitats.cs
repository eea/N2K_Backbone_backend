using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;
        public Boolean? PriorityForm { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.SiteCode, c.Version, c.HabitatCode });
        }
    }
}
