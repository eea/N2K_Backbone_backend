using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Sites : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public Boolean? Current { get; set; }
        public string? Name { get; set; }
        public DateTime? CompilationDate { get; set; }
        public DateTime? ModifyTS { get; set; }
        public SiteChangeStatus? CurrentStatus { get; set; }
        public string? CountryCode { get; set; }
        public string? SiteType { get; set; }
        public double? AltitudeMin { get; set; }
        public double? AltitudeMax { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public int? N2KVersioningRef { get; set; }
        public decimal? Area { get; set; }
        public decimal? Length { get; set; }
        public Boolean? JustificationRequired { get; set; }
        public Boolean? JustificationProvided { get; set; }
        public DateTime? DateConfSCI { get; set; }
        public Boolean? Priority { get; set; }
        public DateTime? DatePropSCI { get; set; }
        public DateTime? DateSpa { get; set; }
        public DateTime? DateSac { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string? SpaLegalReference { get; set; }
        public string? SacLegalReference { get; set; }
        public string? Explanations { get; set; }
        public decimal? MarineArea { get; set; }

        private string dbConnection = string.Empty;

        public Sites() { }

        public Sites(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<Sites> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Sites";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Sites>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Sites - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sites>()
                .ToTable("Sites")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}