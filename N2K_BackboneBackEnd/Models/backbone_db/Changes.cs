using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.BackboneDB
{
    /// <summary>
    /// Class to manage the Entity Changes
    /// </summary>
    public class Changes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Country { get; set; }
        public SiteChangeStatus? Status { get; set; }
        public string? Tags { get; set; }
        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }
        public string? NewValue { get; set; }
        public string? OldValue { get; set; }
        public string? Detail { get; set; }
        public List<SiteChangeView> Subrows { get; set; } = new List<SiteChangeView>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Changes>()
                .ToTable("Changes4Sites")
                .HasKey(c => c.Id);
            builder.Entity<Changes>()
                .ToTable("Changes4Sites")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());
            builder.Entity<Changes>()
                .ToTable("Changes4Sites")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }
}