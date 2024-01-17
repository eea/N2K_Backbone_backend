using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteActivities : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Action { get; set; } = string.Empty;
        public Boolean? Deleted { get; set; }

        private string dbConnection = "";

        public SiteActivities() { }

        public SiteActivities(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<SiteActivities> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SiteActivities";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SiteActivities>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteActivities - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteActivities>()
                .ToTable("SiteActivities")
                .HasKey(c => new { c.ID });
        }
    }
}