using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class LineageExtended : Lineage
    {
        public string? Name { get; set; }
        public string? AntecessorsSiteCodes { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageExtended>();
        }
    }
}
