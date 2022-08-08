using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteGeometryDetailed : IEntityModel
    {
        
        public string? SiteCode { get; set; } = "";
        public int Version { get; set; }
        public string ReportedGeom { get; set; } = "";
        public string ReferenceGeom { get; set; } = "";
        public string ChangedGeom { get; set; } = "";
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteGeometryDetailed>();
        }
       

    }
}
