using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
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

    [Keyless]
    public class SpeciesToHarvestPerEnvelope : SpeciesToHarvest, IEntityModel
    {
        public string? SiteCode { get; set; } = string.Empty;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesToHarvestPerEnvelope>()
                .HasNoKey();
        }
    }
}