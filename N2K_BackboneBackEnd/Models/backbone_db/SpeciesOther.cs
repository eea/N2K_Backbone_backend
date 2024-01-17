using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesOther : IEntityModel, IEntityModelBackboneDB
    {
        public long Id { get; set; }
        public string SiteCode { get; set; }
        public int Version { get; set; }
        public string? SpecieCode { get; set; }
        public int? PopulationMin { get; set; }
        public int? PopulationMax { get; set; }
        public string? Group { get; set; }
        public Boolean? SensitiveInfo { get; set; }
        public string? Resident { get; set; }
        public string? Breeding { get; set; }
        public string? Winter { get; set; }
        public string? Staging { get; set; }
        public string? Path { get; set; }
        public string? AbundaceCategory { get; set; }
        public string? Motivation { get; set; }
        public string? PopulationType { get; set; }
        public string? CountingUnit { get; set; }
        public string? Population { get; set; }
        public string? Insolation { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public Boolean? NonPersistence { get; set; }
        public string? DataQuality { get; set; }
        public string? SpecieType { get; set; }

        private string dbConnection = "";

        public SpeciesOther() { }

        public SpeciesOther(string db)
        {
            dbConnection = db;
        }

        public async static Task<int> SaveBulkRecord(string db, List<SpeciesOther> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SpeciesOther";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SpeciesOther>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SpeciesOther - SaveBulkRecord", "", db);
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SpeciesOther>()
                .ToTable("SpeciesOther")
                .HasKey(c => new { c.Id });
        }
    }
}