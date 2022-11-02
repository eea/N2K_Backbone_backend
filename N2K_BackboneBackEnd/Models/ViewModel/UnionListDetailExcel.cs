using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class UnionListDetailExcel : IEntityModel
    {
        public string SCI_code { get; set; } = string.Empty;
        public string SCI_Name { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }
        public double? Long { get; set; }
        public double? Lat { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListDetailExcel>();
        }
    }
}
