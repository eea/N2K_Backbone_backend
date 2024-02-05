using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public abstract class DocumentationChanges
    {
        [Key]
        public long Id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public abstract string? Tags { get; set; }
        [NotMapped]
        public bool Temporal { get; set; } = false;
    }
}