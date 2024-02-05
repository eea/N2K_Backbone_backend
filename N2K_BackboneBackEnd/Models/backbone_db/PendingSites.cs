using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class PendingSites : IEntityModel, IEntityModelBackboneDB
    {
        public int NumSites { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PendingSites>();
        }
    }
}