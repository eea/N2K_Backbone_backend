using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public class SpeciesToHarvest : IEntityModel
    {
        public string? SpeciesCode { get; set; } = string.Empty;
        public int VersionId { get; set; }
        public string? Population { get; set; } = string.Empty;
        public string? PopulationType { get; set; } = string.Empty;
        public string? Motivation { get; set; } = string.Empty;
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesToHarvest>()
                .HasNoKey();
        }
    }
}
