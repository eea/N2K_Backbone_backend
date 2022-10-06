using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class BioRegionTypes : IEntityModel, IEntityModelBackboneDB
    {
        public int Code { get; set; }
        public string? RefBioGeoName { get; set; }
        public string? RefBioRegionCode { get; set; }
        public string? BioRegionShortCode { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegionTypes>()
                .ToTable("BioRegionTypes")
                .HasKey(c => new { c.Code });
        }
    }
}
