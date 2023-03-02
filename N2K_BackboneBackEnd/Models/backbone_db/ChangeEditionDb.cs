using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class ChangeEditionDb :  IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = "";
        public int Version { get; set; }
        public string? SiteName { get; set; }
        public string? SiteType { get; set; }
        //public List<int> BioRegion { get; set; } = new List<int>();
        //public string? spBiogeographicRegion { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }
        public double? CentreX { get; set; }
        public double? CentreY { get; set; }

        public string? BioRegion { get; set; }

        public bool? JustificationRequired { get; set; } = false;

        public bool? JustificationProvided { get; set; } = false;

        public static  void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionDb>();
        }

    }
}
