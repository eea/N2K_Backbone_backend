using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class StatusChanges : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime? Date { get; set; }
        public string? Owner { get; set; }
        public string? Justification { get; set; }
        public string? Comments { get; set; }
        public string? Tags { get; set; }
        public int Edited { get; set; }
        public DateTime? EditedDate { get; set; }
        public string? Editedby {  get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<StatusChanges>()
                .ToTable("StatusChanges");
        }
    }
}
