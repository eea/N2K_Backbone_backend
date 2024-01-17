using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class HasNationalProtection : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? DesignatedCode { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percentage { get; set; }

        private string dbConnection = "";

        public HasNationalProtection() { }

        public HasNationalProtection(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<HasNationalProtection> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "HasNationalProtection";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<HasNationalProtection>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HasNationalProtection - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HasNationalProtection>()
                .ToTable("HasNationalProtection")
                .HasKey(c => new { c.ID });
        }
    }
}