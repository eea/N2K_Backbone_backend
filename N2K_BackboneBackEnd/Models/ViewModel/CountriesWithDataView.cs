using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class CountriesWithDataView : IEntityModel
    {
        public string Code { get; set; } = "";
        public string? Country { get; set; }
        public bool? isEUCountry { get; set; }
        public int Version { get; set; } = 0;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountriesWithDataView>();
        }
    }
}