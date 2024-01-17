using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class IsImpactedBy : IEntityModel, IEntityModelBackboneDB
    {
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? ActivityCode { get; set; }
        public string? InOut { get; set; }
        public string? Intensity { get; set; }
        public double? PercentageAff { get; set; }
        public string? Influence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PollutionCode { get; set; }
        public string? Ocurrence { get; set; }
        public string? ImpactType { get; set; }
        public long Id { get; set; }

        private string dbConnection = "";

        public IsImpactedBy() { }

        public IsImpactedBy(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<IsImpactedBy> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "IsImpactedBy";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<IsImpactedBy>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "IsImpactedBy - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .ToTable("IsImpactedBy")
                .HasKey(c => new { c.Id });
        }
    }
}