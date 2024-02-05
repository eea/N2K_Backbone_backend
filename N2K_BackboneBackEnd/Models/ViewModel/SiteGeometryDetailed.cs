using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteGeometryDetailed : IEntityModel
    {
        public string? SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string ReportedGeom { get; set; } = string.Empty;
        public string ReferenceGeom { get; set; } = string.Empty;
        public string ChangedGeom { get; set; } = string.Empty;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteGeometryDetailed>();
        }
    }
}