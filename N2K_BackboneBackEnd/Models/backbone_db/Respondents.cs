using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Respondents : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ID { get; }
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? locatorName { get; set; }
        public string? addressArea { get; set; }
        public string? postName { get; set; }
        public string? postCode { get; set; }
        public string? thoroughfare { get; set; }
        public string? addressUnstructured { get; set; }
        public string? name { get; set; }
        public string? Email { get; set; }
        public string? AdminUnit { get; set; }
        public string? LocatorDesignator { get; set; }


        private string dbConnection = "";

        public Respondents() { }

        public Respondents(string db)
        {
            dbConnection = db;
        }


        public async static Task<int> SaveBulkRecord(string db, List<Respondents> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Respondents";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Respondents>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Respondents - SaveBulkRecord", "", db);
                return 0;
            }

        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Respondents>()
                .ToTable("Respondents")
                .HasKey("ID");

        }

    }
}
