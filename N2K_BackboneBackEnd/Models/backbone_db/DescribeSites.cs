using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DescribeSites : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? HabitatCode { get; set; } = string.Empty;
        public decimal? Percentage { get; set; }

        private string dbConnection = string.Empty;

        public DescribeSites() { }

        public DescribeSites(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<DescribeSites> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "DescribeSites";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<DescribeSites>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DescribeSites - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DescribeSites>()
                .Property(b => b.Percentage)
                .HasPrecision(18, 2);


            builder.Entity<DescribeSites>()
                .ToTable("DescribeSites")
                .HasKey(c => new { c.ID });
        }
    }
}