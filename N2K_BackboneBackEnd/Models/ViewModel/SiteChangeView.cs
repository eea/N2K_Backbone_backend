using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class SiteChangeView : IEntityModel
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
        public string? Action { get; set; } = string.Empty;
        [NotMapped]
        public int NumChanges { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeView>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeView>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }
}