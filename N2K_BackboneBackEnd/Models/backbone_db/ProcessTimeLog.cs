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


        public ProcessTimeLog(string pProcessName, string pActionPerformed) { 
            this.ProcessName = pProcessName;
            this.ActionPerformed = pActionPerformed;
            this.StampTime = DateTime.Now;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessTimeLog>()
                .ToTable("ProcessTimeLog")
                .HasKey(c => c.Id);
        }

    }
}
