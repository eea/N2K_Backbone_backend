using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public class HabitatToHarvest : IEntityModel
    {
        public string HabitatCode { get; set; } = string.Empty;
        public int VersionId { get; set; }
        public string? RelSurface { get; set; }
        public string? Representativity { get; set; }
        public double? Cover_ha { get; set; }
        //public string? HabitatType { get; set; }
        public bool? PriorityForm { get; set; } // "PF" in the versioning DB
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatToHarvest>()
                .HasNoKey();
        }
    }
}
