using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Lineage : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public long Version { get; set; }
        public long? AntecessorsVersion { get; set; }
        public string? AntecessorsSiteCodes { get; set; } = string.Empty;
        public LineageTypes? Type { get; set; }
        public LineageStatus? Status { get; set; }

        public SiteChangeStatus Status { get; set; }

        private string dbConnection = "";

        public Lineage() { }

        public Lineage(string db)
        {
            dbConnection = db;
        }


        public void SaveRecord(string db)
        {
            try
            {
                this.dbConnection = db;
                SqlConnection conn = null;
                SqlCommand cmd = null;

                conn = new SqlConnection(this.dbConnection);
                conn.Open();
                cmd = conn.CreateCommand();
                SqlParameter param1 = new SqlParameter("@SiteCode", this.SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", this.Version);
                SqlParameter param3 = new SqlParameter("@AntecessorsVersion", this.AntecessorsVersion is null ? DBNull.Value : this.AntecessorsVersion);
                SqlParameter param4 = new SqlParameter("@AntecessorsSiteCodes", this.AntecessorsSiteCodes is null ? DBNull.Value : this.AntecessorsSiteCodes);
                SqlParameter param5 = new SqlParameter("@Type", this.Type is null ? DBNull.Value : this.Type);
                SqlParameter param6 = new SqlParameter("@Status", this.Status is null ? DBNull.Value : this.Status);

                cmd.CommandText = "INSERT INTO [Lineage] (  " +
                    "[SiteCode],[Version],[AntecessorsVersion],[AntecessorsSiteCodes],[Type],[Status]) " +
                    " VALUES (@SiteCode,@Version,@AntecessorsVersion,@AntecessorsSiteCodes,@Type,@Status) ";

                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Lineage - SaveRecord", "");
            }
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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Lineage - SaveBulkRecord", "");
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
