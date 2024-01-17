using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteGeometry : IEntityModel
    {
        public string? GeoJson { get; set; } = string.Empty;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteGeometry>();
        }
    }
}