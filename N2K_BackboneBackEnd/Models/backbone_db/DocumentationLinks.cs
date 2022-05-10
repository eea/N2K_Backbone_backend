using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DocumentationLinks : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? Link { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DocumentationLinks>()
                .ToTable("DocumentationLinks")
                .HasKey(c => new { c.ID });
        }
    }
}
