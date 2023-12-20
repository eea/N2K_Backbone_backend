using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteLargeDescriptions : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Quality { get; set; }
        public string? Vulnarab { get; set; }
        public string? Designation { get; set; }
        public string? ManagPlan { get; set; }
        public string? Documentation { get; set; }
        public string? OtherCharact { get; set; }
        public string? ManagConservMeasures { get; set; }
        public string? ManagPlanUrl { get; set; }
        public string? ManagStatus { get; set; }
        public long ID { get; set; }

        private string dbConnection = "";

        public SiteLargeDescriptions() { }

        public SiteLargeDescriptions(string db)
        {
            dbConnection = db;
        }


        public async static Task<int> SaveBulkRecord(string db, List<SiteLargeDescriptions> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SiteLargeDescriptions";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SiteLargeDescriptions>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLargeDescriptions - SaveBulkRecord", "", db);
                return 0;
            }
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteLargeDescriptions>()
                .ToTable("SiteLargeDescriptions")
                .HasKey(c => c.ID );
        }
    }
}
