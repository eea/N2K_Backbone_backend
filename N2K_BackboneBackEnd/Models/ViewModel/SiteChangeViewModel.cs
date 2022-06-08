using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;


namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteChangeViewModel : SiteChangeView, IEntityModel
    {
        public List<SiteChangeView>? Subrows { get; set; } = new List<SiteChangeView>();

        public SiteChangeViewModel()
        {
            this.Subrows = new List<SiteChangeView> ();
        }


        new public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDb>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDb>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }
}
