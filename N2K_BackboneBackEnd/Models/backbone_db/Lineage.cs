using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Lineage : IEntityModel, IEntityModelBackboneDB
    {
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public LineageTypes Type { get; set; }
        public LineageStatus Status { get; set; }
        public long? Release { get; set; }
        [NotMapped]
        public String? AntecessorsSiteCodes { get; set; }


        private string dbConnection = "";

        public Lineage() { }

        public Lineage(string db)
        {
            dbConnection = db;
        }


        public async static Task<int> SaveBulkRecord(string db, List<Lineage> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Lineage";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Lineage>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Lineage - SaveBulkRecord", "", db);
                return 0;
            }

        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Lineage>()
                .ToTable("Lineage")
                .HasKey(c => new { c.ID });
        }
    }
}
