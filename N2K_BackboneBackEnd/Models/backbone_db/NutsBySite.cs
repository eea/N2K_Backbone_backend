using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class NutsBySite : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string NutId { get; set; } = string.Empty;
        public double? CoverPercentage { get; set; }

        private string dbConnection = "";

        public NutsBySite() { }

        public NutsBySite(string db)
        {
            dbConnection = db;
        }


        public async static Task<int> SaveBulkRecord(string db, List<NutsBySite> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "NutsBySite";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<NutsBySite>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "NutsBySite - SaveBulkRecord", "", db);
                return 0;
            }
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsBySite>()
                .ToTable("NutsBySite")
                .HasKey(c => new { c.SiteCode, c.Version, c.NutId });
        }
    }
}
