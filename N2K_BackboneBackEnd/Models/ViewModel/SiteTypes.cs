using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class SiteTypes : IEntityModel, IEntityModelBackboneDB
    {
        public string Code { get; set; } = string.Empty;
        public string? Classification { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteTypes>()
                .ToTable("SiteTypes")
                .HasKey(c => new { c.Code });
        }
    }
}