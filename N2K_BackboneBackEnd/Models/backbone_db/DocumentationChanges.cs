using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DocumentationChanges : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Tags { get; set; }
        [NotMapped]
        public bool Temporal { get; set; } = false;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<StatusChanges>();
        }
    }


}
