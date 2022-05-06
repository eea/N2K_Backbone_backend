namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class HasNationalProtection : IEntityModel
    {
        public int ID { get; set; };
        public string? SiteCode { get; set; };
        public int? Version { get; set; };
        public string? DesignatedCode { get; set; };
        public double? Percentage { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HasNationalProtection>()
                .HasNoKey();
        }
    }
}
