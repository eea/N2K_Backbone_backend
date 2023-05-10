using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class HasNationalProtection : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
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
                SqlParameter param1 = new SqlParameter("@SiteCode", this.SiteCode is null ? DBNull.Value : this.SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", this.Version is null ? DBNull.Value : this.Version);
                SqlParameter param3 = new SqlParameter("@DesignatedCode", this.DesignatedCode is null ? DBNull.Value : this.DesignatedCode);
                SqlParameter param4 = new SqlParameter("@Percentage", this.Percentage is null ? DBNull.Value : this.Percentage);

                cmd.CommandText = "INSERT INTO [HasNationalProtection] (  " +
                    "[SiteCode],[Version],[DesignatedCode],[Percentage]) " +
                    " VALUES (@SiteCode,@Version,@DesignatedCode,@Percentage) ";

                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HasNationalProtection - SaveRecord", "");
            }
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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HasNationalProtection - SaveBulkRecord", "");
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
