using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteChangeDbEdition : SiteChangeDb, IEntityModel, IEntityModelBackboneDB
    {
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }

        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }
}