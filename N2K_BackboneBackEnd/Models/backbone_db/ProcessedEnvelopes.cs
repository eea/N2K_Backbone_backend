using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.BackboneDB
{
    public class ProcessedEnvelopes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; }

        public DateTime ImportDate { get; protected  set; }
        public string? Country { get; protected set; }

        public int Version { get; protected  set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessedEnvelopes>()
                .ToTable("vLatestProcessedEnvelopes")
                .HasKey("Id");


        }
       
    }
}
