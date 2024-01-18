using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class UnionListDetailExcel : IEntityModel
    {
        public string SCI_code { get; set; } = string.Empty;
        public string SCI_Name { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public string? Area { get; set; }
        public string? Length { get; set; }
        public string? Long { get; set; }
        public string? Lat { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListDetailExcel>();
        }
    }
}