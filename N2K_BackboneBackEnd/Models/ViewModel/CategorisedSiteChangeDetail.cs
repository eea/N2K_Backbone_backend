using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;
namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CategorisedSiteChangeDetail
    {
        public List<CategoryChangeDetail> SiteInfo { get; set; } = new List<CategoryChangeDetail>();
        public List<CategoryChangeDetail> Species { get; set; } = new List<CategoryChangeDetail>();
        public List<CategoryChangeDetail> Habitats { get; set; } = new List<CategoryChangeDetail>();

        public Level? Level { get; set; } = Enumerations.Level.Info;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CategorisedSiteChangeDetail>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }



}
