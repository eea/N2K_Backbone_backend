using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.BackboneDB
{
    public class SiteChangeDb : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ChangeId { get; set; }

        public string? SiteCode { get; set; }
        public string? Country { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

        [NotMapped]
        public int NumChanges { get; set; }

        public string ? NewValue { get; set; }
        public string? OldValue { get; set; }


        public List<SiteChangeView> Subrows { get; set;  } = new List<SiteChangeView>();


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDb>()
                .ToTable("test_table")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDb>()
                .ToTable("test_table")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());


        }
    }
}
