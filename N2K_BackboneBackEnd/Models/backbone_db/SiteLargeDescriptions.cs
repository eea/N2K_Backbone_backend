namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteLargeDescriptions : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; };
        public string? Quality { get; set; };
        public string? Vulnarab { get; set; };
        public string? Designation { get; set; };
        public string? ManagPlan { get; set; };
        public string? Documentation { get; set; };
        public string? OtherCharact { get; set; };
        public string? ManagConservMeasures { get; set; };
        public string? ManagPlanUrl { get; set; };
        public string? ManagStatus { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteLargeDescriptions>()
                .HasNoKey();
        }
    }
}
