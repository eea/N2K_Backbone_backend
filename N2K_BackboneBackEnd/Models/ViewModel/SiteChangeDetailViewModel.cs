using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;


namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class SiteChangeDetailViewModel :  IEntityModel
    {

        public string SiteCode { get; set; } = "";

        public string Name { get; set; } = "";

        public int CountryVersion { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public List<ChangeDetail> ChangesList { get; set; } = new List<ChangeDetail>();


        public SiteChangeDetailViewModel()
        {
            this.ChangesList = new List<ChangeDetail>();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDetailViewModel>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());
        }

    }
}
