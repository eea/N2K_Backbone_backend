using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DocumentationLinks : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? Link { get; set; }

        private string dbConnection = string.Empty;

        public DocumentationLinks() { }

        public DocumentationLinks(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<DocumentationLinks> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "DocumentationLinks";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<DocumentationLinks>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DocumentationLinks - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DocumentationLinks>()
                .ToTable("DocumentationLinks")
                .HasKey(c => new { c.ID });
        }
    }
}