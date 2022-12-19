using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{


    public class UnionListHeaderInputParam : IEntityModel, IEntityModelBackboneDB
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;     
        public Boolean? Final { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListHeaderInputParam>()
                //.ToTable("UnionListHeader")
                .HasNoKey();
        }
    }

    public class UnionListHeader : IEntityModel, IEntityModelBackboneDB
    {
        public long idULHeader { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string? CreatedBy { get; set; }
        public Boolean? Final { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public long? ReleaseID { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListHeader>()
                .ToTable("UnionListHeader")
                .HasKey(c => new { c.idULHeader });
        }
    }
}
