namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DetailedProtectionStatus : IEntityModel
    {
        public string? SiteCode { get; set; };
        public int? Version { get; set; };
        public string? DesignationCode { get; set; };
        public string? Name { get; set; };
        public int ID { get; set; };
        public string? OverlapCode { get; set; };
        public double? OverlapPercentage { get; set; };
        public string? Convention { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DetailedProtectionStatus>()
                .HasNoKey();
        }
    }
}
