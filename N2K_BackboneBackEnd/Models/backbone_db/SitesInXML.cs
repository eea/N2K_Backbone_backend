using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SitesInXML : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime? Date { get; set; }
        [NotMapped]
        public string? XMLContent { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SitesInXML>()
                .ToTable("SitesInXML")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}