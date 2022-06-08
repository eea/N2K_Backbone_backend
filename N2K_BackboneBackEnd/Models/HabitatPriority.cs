using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public class HabitatPriority : IEntityModel
    {
        public string HabitatCode { get; set; } = string.Empty;
        public int? Priority { get; set; }
        public string? Source { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatPriority>()
                .HasNoKey();
        }
    }
}
