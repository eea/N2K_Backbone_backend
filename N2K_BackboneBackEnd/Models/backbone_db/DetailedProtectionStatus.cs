using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DetailedProtectionStatus : IEntityModel, IEntityModelBackboneDB
    {
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? DesignationCode { get; set; }
        public string? Name { get; set; }
        public long ID { get; set; }
        public string? OverlapCode { get; set; }
        public decimal? OverlapPercentage { get; set; }
        public string? Convention { get; set; }

        private string dbConnection = "";

        public DetailedProtectionStatus() { }

        public DetailedProtectionStatus(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<DetailedProtectionStatus> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "DetailedProtectionStatus";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<DetailedProtectionStatus>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DetailedProtectionStatus - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DetailedProtectionStatus>()
                .Property(b => b.OverlapPercentage)
                .HasPrecision(38, 2);

            builder.Entity<DetailedProtectionStatus>()
                .ToTable("DetailedProtectionStatus")
                .HasKey(c => new { c.ID });
        }
    }
}