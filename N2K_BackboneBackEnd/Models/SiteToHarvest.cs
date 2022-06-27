using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    public class SiteToHarvest : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int VersionId { get; set; }
        public string? SiteName { get; set; }
        public double? AreaHa { get; set; }
        //public string? PriorityLevel { get; set; }
        public double? LengthKm { get; set; }
        public string? SiteType { get; set; }
        public string? CountryCode { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteToHarvest>()
                .HasNoKey();
        }
    }
}
