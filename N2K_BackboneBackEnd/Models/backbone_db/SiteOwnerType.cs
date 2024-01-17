using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteOwnerType : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Type { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percent { get; set; }

        private string dbConnection = "";

        public SiteOwnerType() { }

        public SiteOwnerType(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<SiteOwnerType> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SiteOwnerType";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SiteOwnerType>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteOwnerType - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteOwnerType>()
                .ToTable("SiteOwnerType")
                .HasKey(c => new { c.SiteCode, c.Version, c.Type });
        }
    }
}