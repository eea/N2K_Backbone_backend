using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class BioRegions : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public int BGRID { get; set; }
        public double? Percentage { get; set; }
        public Boolean? isMarine { get; set; }


        private string dbConnection = "";

        public BioRegions() { }

        public BioRegions(string db)
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
                SqlParameter param3 = new SqlParameter("@BGRID", this.BGRID);
                SqlParameter param4 = new SqlParameter("@Percentage", this.Percentage is null ? DBNull.Value : this.Percentage);
                SqlParameter param5 = new SqlParameter("@isMarine", this.isMarine is null ? DBNull.Value : this.isMarine);

                cmd.CommandText = "INSERT INTO [BioRegions] (  " +
                    "[SiteCode],[Version],[BGRID],[Percentage],[isMarine]) " +
                    " VALUES (@SiteCode,@Version,@BGRID,@Percentage,@isMarine) ";

                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "BioRegions - SaveRecord", "");
            }
        }

        public async static Task<int> SaveBulkRecord(string db, List<BioRegions> listData)
        {

            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "BioRegions";
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<BioRegions>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "BioRegions - SaveBulkRecord", "");
                return 0;
            }
            
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegions>()
                .ToTable("BioRegions")
                .HasKey(c => new { c.SiteCode, c.Version, c.BGRID });
        }
    }
}
