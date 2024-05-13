using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.releases_db
{
    public class HabitatsRelease : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long ReleaseId { get; set; }
        public string? CountryCode { get; set; }
        public string? SiteCode { get; set; }
        public string? HabitatCode { get; set; }
        public string? Description { get; set; }
        public string? HabitatPriority { get; set; }
        public bool? PriorityFormHabitatType { get; set; }
        public int? NonPresenceInSite { get; set; }
        public double? CoverHa { get; set; }
        public string? Caves { get; set; }
        public string? Representativity { get; set; }
        public string? Relsurface { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public string? Dataquality { get; set; }
        public double? PercentageCover { get; set; }
        public bool? IntroductionCandidate { get; set; }

        private string dbConnection = string.Empty;

        public HabitatsRelease() { }

        public HabitatsRelease(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatsRelease>()
                .ToTable("HABITATS")
                .HasKey(c => new { c.id });
        }
    }
}
