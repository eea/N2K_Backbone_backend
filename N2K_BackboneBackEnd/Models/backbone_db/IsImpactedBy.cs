namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class IsImpactedBy : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; };
        public string ActivityCode { get; set; };
        public string? InOut { get; set; };
        public string? Intensity { get; set; };
        public double? PercentageAff { get; set; };
        public string? Influence { get; set; };
        public DateOnly? StartDate { get; set; };
        public DateOnly? EndDate { get; set; };
        public string? PollutionCode { get; set; };
        public string? Ocurrence { get; set; };
        public string? ImpactType { get; set; };
        public int Id { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .HasNoKey();
        }
    }
}
