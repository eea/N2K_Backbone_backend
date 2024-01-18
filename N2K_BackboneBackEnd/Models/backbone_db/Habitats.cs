using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? CoverHA { get; set; }
        public Boolean? PriorityForm { get; set; }
        public string? Representativity { get; set; }
        public int? DataQty { get; set; }
        public string? Conservation { get; set; }
        public string? GlobalAssesments { get; set; }
        public string? RelativeSurface { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percentage { get; set; }
        public string? ConsStatus { get; set; }
        public string? Caves { get; set; }
        public string? PF { get; set; }
        public int? NonPresenciInSite { get; set; }

        private string dbConnection = string.Empty;

        public Habitats() { }

        public Habitats(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<Habitats> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Habitats";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Habitats>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Habitats - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.id });
        }
    }
}