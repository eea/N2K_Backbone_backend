using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class ProcessTimeLog : IEntityModel, IEntityModelBackboneDB
    {
        public long Id { get; set; }
        public string ProcessName { get; set; }
        public string ActionPerformed { get; set; }
        public DateTime StampTime { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessTimeLog>()
                .ToTable("ProcessTimeLog")
                .HasKey(c => c.Id);
        }

    }
}
