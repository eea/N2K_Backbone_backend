using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class ReleaseDetail : IEntityModel, IEntityModelReleasesDB
    {
        public long? idReleaseDetail { get; set; }
        public long idReleaseHeader { get; set; }
        public string SCI_code { get; set; } = string.Empty;
        public string BioRegion { get; set; } = string.Empty;
        public string SCI_Name { get; set; } = string.Empty;
        public Boolean? Priority { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }
        public double? Long { get; set; }
        public double? Lat { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleaseDetail>().HasNoKey();
        }
    }
}
