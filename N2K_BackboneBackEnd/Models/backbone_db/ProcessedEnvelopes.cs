using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.BackboneDB
{
    public class ProcessedEnvelopes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; }

        public DateTime ImportDate { get;   set; }
        public string? Country { get;  set; }

        public int Version { get;   set; }

        public int Status { get;  set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessedEnvelopes>()
                .ToTable("ProcessedEnvelopes")
                .HasKey("Id");

        }
       
    }
}
