using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class SiteChangeExtended : IEntityModel
    {

        public int ChangeId { get; set; }

        public string? SiteCode { get; set; }
        public string? Country { get; set; }


        public Status? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

        public int? Extended1 { get; set; }
        public int? Extended2 { get; set; }


        void IEntityModel.OnModelCreating(ModelBuilder builder)
        {
            OnModelCreating(builder);
        }

        public static  void OnModelCreating(ModelBuilder builder)
        {
            //definition of the DB entities, sources and enumerations
            builder.Entity<SiteChangeExtended>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.Status>());

            builder.Entity<SiteChangeExtended>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }

    }
}
