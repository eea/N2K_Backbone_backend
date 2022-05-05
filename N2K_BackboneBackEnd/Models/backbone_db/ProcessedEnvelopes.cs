using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.BackboneDB
{
    public class ProcessedEnvelopes : IEntityModel
    {
        [Key]
        public long Id { get; set; }

        public DateTime ImportDate { get; set; }
        public string? Country { get; set; }

        public int Version { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessedEnvelopes>()
                .ToView("vLatestProcessedEnvelopes");
        }
    }
}
