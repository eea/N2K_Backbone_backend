using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Nuts : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public string Code { get; set; }
        public string? Region { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Nuts>()
                .ToTable("Nuts")
                .HasKey(c => c.Code);
        }
    }
}
