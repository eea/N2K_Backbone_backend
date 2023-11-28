using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
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
