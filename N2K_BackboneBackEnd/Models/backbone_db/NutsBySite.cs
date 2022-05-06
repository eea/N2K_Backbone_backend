namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class NutsBySite : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; };
        public string NutId { get; set; };
        public double? CoverPercentage { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsBySite>()
                .HasNoKey();
        }
    }
}
