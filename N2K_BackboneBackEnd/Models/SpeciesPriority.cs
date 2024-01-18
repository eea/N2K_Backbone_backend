using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public class SpeciesPriority : IEntityModel
    {
        public string SpecieCode { get; set; } = string.Empty;
        public string? TaxGroup { get; set; }
        public int? Priority { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesPriority>()
                .HasNoKey();
        }
    }
}