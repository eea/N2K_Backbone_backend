using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class ChangeEditionDb : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = "";
        public int Version { get; set; }
        public string? SiteName { get; set; }
        public string? SiteType { get; set; }
        //public List<int> BioRegion { get; set; } = new List<int>();
        //public string? spBiogeographicRegion { get; set; }
        public decimal? Area { get; set; }
        public decimal? Length { get; set; }
        public decimal? CentreX { get; set; }
        public decimal? CentreY { get; set; }
        public string? BioRegion { get; set; }
        public bool? JustificationRequired { get; set; } = false;
        public bool? JustificationProvided { get; set; } = false;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionDb>();
        }
    }
}