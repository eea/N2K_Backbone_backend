using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class ChangeEditionDb : ChangeEdition, IEntityModel, IEntityModelBackboneDB
    {
        public string? BioRegion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionDb>();
        }

    }
}
