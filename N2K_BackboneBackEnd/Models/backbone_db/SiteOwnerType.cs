using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteOwnerType : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int Type { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percent { get; set; }

        private string dbConnection = "";

        public SiteOwnerType() { }

        public SiteOwnerType(string db)
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
                SqlParameter param3 = new SqlParameter("@Type", this.Type);
                SqlParameter param4 = new SqlParameter("@Percent", this.Percent is null ? DBNull.Value : this.Percent);

                cmd.CommandText = "INSERT INTO [SiteOwnerType] (  " +
                    "[SiteCode],[Version],[Type],[Percent]) " +
                    " VALUES (@SiteCode,@Version,@Type,@Percent) ";

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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteOwnerType - SaveRecord", "");
            }
        }

        public async  static Task<int> SaveBulkRecord(string db, List<SiteOwnerType> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SiteOwnerType";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SiteOwnerType>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteOwnerType - SaveBulkRecord", "");
                return 0;
            }
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteOwnerType>()
                .ToTable("SiteOwnerType")
                .HasKey(c => new { c.SiteCode, c.Version, c.Type });
        }
    }
}
