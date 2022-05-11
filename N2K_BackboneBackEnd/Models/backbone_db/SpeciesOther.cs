using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesOther : IEntityModel, IEntityModelBackboneDB
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string SpecieCode { get; set; } = string.Empty;
        public int? PopulationMin { get; set; }
        public int? PopulationMax { get; set; }
        public string? Group { get; set; }
        public Boolean? SensitiveInfo { get; set; }
        public string? Resident { get; set; }
        public string? Breeding { get; set; }
        public string? Winter { get; set; }
        public string? Staging { get; set; }
        public string? Path { get; set; }
        public string? AbundaceCategory { get; set; }
        public string? Motivation { get; set; }
        public string? PopulationType { get; set; }
        public string? CountingUnit { get; set; }
        public string? Population { get; set; }
        public string? Insolation { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public Boolean? NonPersistence { get; set; }
        public string? DataQuality { get; set; }
        public string? SpecieType { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesOther>()
                .ToTable("SpeciesOther")
                .HasKey(c => new { c.id });
        }
    }
}
