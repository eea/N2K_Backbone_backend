using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class StatusChanges : DocumentationChanges , IEntityModel, IEntityModelBackboneDB
    {
        //[Key]
        //public long Id { get; set; }
        //public string SiteCode { get; set; } = string.Empty;
        //public int Version { get; set; }
        public DateTime? Date { get; set; }
        public string? Owner { get; set; }
        public string? Justification { get; set; }
        
        public string? Comments { get; set; }

        public override string? Tags { get; set; } = "";

        public int? Edited { get; set; }
        public DateTime? EditedDate { get; set; }
        public string? EditedBy {  get; set; }

        //[NotMapped]
        //public bool Temporal { get; set; } = false;

        public static new void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<StatusChanges>()
                .ToTable("StatusChanges");
        }
    }
}
