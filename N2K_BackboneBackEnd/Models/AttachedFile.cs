using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    public class AttachedFile : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AttachedFile>()
                .HasNoKey();
        }
    }
}
