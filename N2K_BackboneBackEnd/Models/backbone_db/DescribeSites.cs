using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Data;


namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DescribeSites : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;
        
        public decimal? Percentage { get; set; }

        private string dbConnection = "";
        public DescribeSites() { }

        public DescribeSites(string db)
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
                SqlParameter param3 = new SqlParameter("@HabitatCode", this.HabitatCode);
                SqlParameter param4 = new SqlParameter("@Percentage", this.Percentage is null ? DBNull.Value : this.Percentage);

                cmd.CommandText = "INSERT INTO [DescribeSites] (  " +
                    "[SiteCode],[Version],[HabitatCode],[Percentage]) " +
                    " VALUES (@SiteCode,@Version,@HabitatCode,@Percentage) ";

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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "DescribeSites - SaveRecord", "");

            }
        }
        public async static Task<int> SaveBulkRecord(string db, List<DescribeSites> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "DescribeSites";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<DescribeSites>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "DescribeSites - SaveBulkRecord", "");
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DescribeSites>()
                .Property(b => b.Percentage)
                .HasPrecision(18, 2);


            builder.Entity<DescribeSites>()
                .ToTable("DescribeSites")
                .HasKey(c => new { c.SiteCode, c.Version, c.HabitatCode });
        }
    }
}
