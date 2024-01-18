using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class LineageAntecessors : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public long LineageID { get; set; }

        private string dbConnection = string.Empty;

        public LineageAntecessors() { }

        public LineageAntecessors(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<LineageAntecessors> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "LineageAntecessors";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<LineageAntecessors>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "LineageAntecessors - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageAntecessors>()
                .ToTable("LineageAntecessors")
                .HasKey(c => new { c.ID });
        }
    }
}