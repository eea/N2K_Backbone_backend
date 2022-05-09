using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class BioRegions : IEntityModel, IEntityModelBackboneDB
    {

        public string SiteCode { get; set; } = string.Empty;

        public int Version { get; set; }

        public int BGRID { get; set; }

        public double? Percentage { get; set; }
        public Boolean? isMarine { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegions>()
                .ToTable("BioRegions")
                .HasKey(c => new { c.SiteCode , c.Version, c.BGRID });
        }
    }
}
