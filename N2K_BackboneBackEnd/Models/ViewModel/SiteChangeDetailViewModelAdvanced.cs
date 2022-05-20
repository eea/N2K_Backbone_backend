using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;


namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class SiteChangeDetailViewModelAdvanced :  IEntityModel
    {

        public string SiteCode { get; set; } = "";

        public string Name { get; set; } = "";

        public int CountryVersion { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public List<CriticalChangeDetail>  Critical { get; set; } = new List<CriticalChangeDetail>();

        public CategorisedSiteChangeDetail Warning { get; set; } = new CategorisedSiteChangeDetail();

        public CategorisedSiteChangeDetail Info { get; set; } = new CategorisedSiteChangeDetail();


        public SiteChangeDetailViewModelAdvanced()
        {
            this.Critical = new List<CriticalChangeDetail>();
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
