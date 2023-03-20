using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteActivities : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        [Key]
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Action { get; set; } = string.Empty;
        public Boolean? Deleted { get; set; }

        private string dbConnection = "";

        public SiteActivities() { }

        public SiteActivities(string db)
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
                SqlParameter param1 = new SqlParameter("@ID", this.ID);
                SqlParameter param2 = new SqlParameter("@SiteCode", this.SiteCode);
                SqlParameter param3 = new SqlParameter("@Version", this.Version);
                SqlParameter param4 = new SqlParameter("@Author", this.Author is null ? DBNull.Value : this.Author);
                SqlParameter param5 = new SqlParameter("@Date", this.Date);
                SqlParameter param6 = new SqlParameter("@Action", this.Action is null ? DBNull.Value : this.Action);
                SqlParameter param7 = new SqlParameter("@Deleted", this.Deleted is null ? DBNull.Value : this.Deleted);

                cmd.CommandText = "INSERT INTO [SiteActivities] (  " +
                    "[SiteCode],[Version],[Author],[Date],[Action],[Deleted]) " +
                    " VALUES (@SiteCode,@Version,@Author,@Date,@Action,@Deleted) ";

                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);
                cmd.Parameters.Add(param7);

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteActivities - SaveRecord", "");
            }
        }
        public static void SaveBulkRecord(string db, List<SiteActivities> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "SiteActivities";
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<SiteActivities>(listData, copy);
                        copy.WriteToServer(data);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteActivities - SaveBulkRecord", "");
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteActivities>()
                .ToTable("SiteActivities")
                .HasKey(c => new { c.ID });
        }
    }
}
