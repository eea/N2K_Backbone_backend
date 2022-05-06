namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteOwnerType : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; };
        public int? Type { get; set; };
        public double? Percent { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteOwnerType>()
                .HasNoKey();
        }
    }
}
