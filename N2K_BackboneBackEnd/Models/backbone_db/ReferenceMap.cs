using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class ReferenceMap : IEntityModel, IEntityModelBackboneDB
    {
        public long Id { get; set; }
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? NationalMapNumber { get; set; }
        public string? Scale { get; set; }
        public string? Projection { get; set; }
        public string? Details { get; set; }
        public string? Inspire { get; set; }
        public Int16? PDFProvided { get; set; }

        private string dbConnection = string.Empty;

        public ReferenceMap() { }

        public ReferenceMap(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<ReferenceMap> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "ReferenceMap";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<ReferenceMap>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReferenceMap - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReferenceMap>()
                .ToTable("ReferenceMap")
                .HasKey(c => new { c.Id });
        }
    }
}