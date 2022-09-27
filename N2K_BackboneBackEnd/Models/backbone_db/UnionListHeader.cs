using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class UnionListHeader : IEntityModel, IEntityModelBackboneDB
    {
        public long idULHeader { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string? CreatedBy { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListHeader>()
                .ToTable("UnionListHeader")
                .HasKey(c => new { c.idULHeader });
        }
    }
}
