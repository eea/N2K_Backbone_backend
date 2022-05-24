using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;


namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class SiteChangeDetailViewModel :  IEntityModel
    {

        public string SiteCode { get; set; } = "";

        public string Name { get; set; } = "";

        public int Version { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public CategorisedSiteChangeDetail  Critical { get; set; } = new CategorisedSiteChangeDetail();

        public CategorisedSiteChangeDetail Warning { get; set; } = new CategorisedSiteChangeDetail();

        public CategorisedSiteChangeDetail Info { get; set; } = new CategorisedSiteChangeDetail();


        public SiteChangeDetailViewModel()
        {
            this.Critical = new CategorisedSiteChangeDetail();
            this.Warning = new CategorisedSiteChangeDetail();
            this.Info= new CategorisedSiteChangeDetail();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDetailViewModel>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());
        }

    }
}
