using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesOther : SpecieBase, IEntityModel, IEntityModelBackboneDB
    {
       
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesOther>()
                .ToTable("SpeciesOther")
                .HasKey(c => new { c.id });
        }
    }
}
