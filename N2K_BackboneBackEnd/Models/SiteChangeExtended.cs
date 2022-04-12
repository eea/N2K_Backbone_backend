using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class SiteChangeExtended : SiteChange, IEntityModel
    {

        public int? Extended1 { get; set; }
        public int? Extended2 { get; set; }

        new public static void OnModelCreating(ModelBuilder builder)
        {
            //definition of the DB entities, sources and enumerations
            builder.Entity<SiteChangeExtended>()
                .HasNoKey()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeExtended>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());

        }

    }
}
