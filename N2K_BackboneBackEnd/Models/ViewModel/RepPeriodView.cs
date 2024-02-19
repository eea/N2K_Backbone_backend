using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class RepPeriodView : RepPeriod
    {
        public List<UnionListHeader>? Releases { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RepPeriod>();
        }
    }
}
