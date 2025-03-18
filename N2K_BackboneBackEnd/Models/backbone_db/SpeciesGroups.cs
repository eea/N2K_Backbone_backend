using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesGroups : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? SpecieHabitat { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesGroups>()
                .ToTable("SpeciesGroups")
                .HasKey(c => new { c.Code });
        }
    }
}