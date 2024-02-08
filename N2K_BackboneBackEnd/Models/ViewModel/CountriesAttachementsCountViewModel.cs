using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class CountriesAttachmentCountViewModel : IEntityModel, IEntityModelReleasesDB
    {
        public string Country {get; set;} = "";
        public string Code {get; set;} = "";
        public int NumDocuments {get; set;} = 0;
        public int NumComments {get; set;} = 0;
        
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountriesAttachmentCountViewModel>()
                .HasNoKey();
        }
    }
}
