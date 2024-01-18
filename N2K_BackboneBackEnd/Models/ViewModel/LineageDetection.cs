using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageDetection : IEntityModel
    {
        public string op { get; set; } = string.Empty;
        public string? old_sitecode { get; set; }
        public int? old_version { get; set; }
        public string? new_sitecode { get; set; }
        public int? new_version { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageDetection>();
        }
    }
}