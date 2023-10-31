using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class RepPeriod : IEntityModel, IEntityModelBackboneDB
    {
        public long Id { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RepPeriod>()
                .ToTable("RepPeriod")
                .HasKey(c => new { c.Id });
        }
    }
}
