using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Species : SpecieBase, IEntityModel, IEntityModelBackboneDB
    {
       
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Species>()
                .ToTable("Species")
                .HasKey(c => new { c.id });
        }
    }
}
