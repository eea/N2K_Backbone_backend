using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class IsImpactedBy : IEntityModel, IEntityModelBackboneDB
    {
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? ActivityCode { get; set; }
        public string? InOut { get; set; }
        public string? Intensity { get; set; }
        public double? PercentageAff { get; set; }
        public string? Influence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PollutionCode { get; set; }
        public string? Ocurrence { get; set; }
        public string? ImpactType { get; set; }
        public long Id { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .ToTable("IsImpactedBy")
                .HasKey(c => new { c.Id });
        }
    }
}
