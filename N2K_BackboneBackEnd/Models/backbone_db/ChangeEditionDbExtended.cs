using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class ChangeEditionDbExtended :  ChangeEditionDb, IEntityModel, IEntityModelBackboneDB
    {
        public DateTime? ReleaseDate { get; set; }
        public static  void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionDbExtended>();
        }
    }
}
