using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class ChangeEdition : IEntityModel, IEntityModelBackboneDB
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
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEdition>();
        }

    }
}
