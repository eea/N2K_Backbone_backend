using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class BioRegions : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int BGRID { get; set; }
        public double? Percentage { get; set; }
        public Boolean? isMarine { get; set; }


        private string dbConnection = "";

        public BioRegions() { }

        public BioRegions(string db)
        {
            dbConnection = db;
        }


        

        public async static Task<int> SaveBulkRecord(string db, List<BioRegions> listData)
        {

            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "BioRegions";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<BioRegions>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "BioRegions - SaveBulkRecord", "", db);
                return 0;
            }
            
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegions>()
                .ToTable("BioRegions")
                .HasKey(c => new { c.SiteCode, c.Version, c.BGRID });
        }
    }
}
