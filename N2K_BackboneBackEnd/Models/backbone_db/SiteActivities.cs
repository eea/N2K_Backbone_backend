using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteActivities : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Action { get; set; } = string.Empty;
        public Boolean? Deleted { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteActivities>()
                .ToTable("SiteActivities")
                .HasKey(c => new { c.ID });
        }
    }
}
