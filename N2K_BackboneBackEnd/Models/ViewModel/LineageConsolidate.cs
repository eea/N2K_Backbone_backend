using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class LineageConsolidate : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public long ID { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public long Version { get; set; }
        public long? AntecessorsVersion { get; set; }
        public string? AntecessorsSiteCodes { get; set; } = string.Empty;
        //public string? SucessorsSiteCodes { get; set; } = string.Empty;
        public LineageTypes? Type { get; set; }
        public LineageStatus? Status { get; set; }


        private string dbConnection = "";

        public LineageConsolidate() { }

        public LineageConsolidate(string db)
        {
            dbConnection = db;
        }


        public void SaveRecord(string db)
        {
            try
            {
                dbConnection = db;
                SqlConnection conn = null;
                SqlCommand cmd = null;

                conn = new SqlConnection(dbConnection);
                conn.Open();
                cmd = conn.CreateCommand();
                SqlParameter param1 = new SqlParameter("@SiteCode", SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", Version);
                SqlParameter param3 = new SqlParameter("@AntecessorsVersion", AntecessorsVersion is null ? DBNull.Value : AntecessorsVersion);
                SqlParameter param4 = new SqlParameter("@AntecessorsSiteCodes", AntecessorsSiteCodes is null ? DBNull.Value : AntecessorsSiteCodes);
                SqlParameter param5 = new SqlParameter("@Type", Type is null ? DBNull.Value : Type);
                SqlParameter param6 = new SqlParameter("@Status", Status is null ? DBNull.Value : Status);

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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "LineageConsolidate - SaveRecord", "");
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
                        DataTable data = listData.PrepareDataForBulkCopy(copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "LineageConsolidate - SaveBulkRecord", "");
                return 0;
            }

        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageConsolidate>();
        }
    }
}
