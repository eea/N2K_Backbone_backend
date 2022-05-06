using System.Data.SqlTypes;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SitesInXML : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; };
        public DateOnly? Date { get; set; };
        public SqlXml? XMLContent { get; set; };
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SitesInXML>()
                .HasNoKey();
        }
    }
}
