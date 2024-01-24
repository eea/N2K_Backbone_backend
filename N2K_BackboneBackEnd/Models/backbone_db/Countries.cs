using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Countries : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string? Country { get; set; }
        public bool? isEUCountry { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Countries>()
                .ToTable("Countries")
                .HasKey(c => new { c.Code });
        }
    }
}