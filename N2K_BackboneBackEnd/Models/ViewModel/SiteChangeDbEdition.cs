using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    public class SiteChangeDbEdition : SiteChangeDb, IEntityModel, IEntityModelBackboneDB
    {

        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public Boolean? Recoded { get; set; }
        public LineageTypes? LineageChangeType { get; set; }

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
