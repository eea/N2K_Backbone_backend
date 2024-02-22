using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    public class AttachedFileRelease : IEntityModel
    {
        public string Country { get; set; } = string.Empty;
        [NotMapped]
        public List<IFormFile>? Files { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AttachedFileRelease>()
                .HasNoKey();
        }
    }
}
