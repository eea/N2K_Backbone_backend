using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    public class SiteChange:IEntityModel
    {
        [Key]
        public int ChangeId { get; set; }

        public string? SiteCode { get; set;  }
        public string? Country { get; set; }

        public Status? Status { get; set; }

        public string? Tags { get; set; }
        
        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChange>()
                .ToTable("test_table")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.Status>());

            builder.Entity<SiteChange>()
                .ToTable("test_table")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }

    }
}
