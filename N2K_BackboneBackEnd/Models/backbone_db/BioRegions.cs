using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BioRegions>()
                .ToTable("BioRegions")
                .HasKey(c => new { c.SiteCode, c.Version, c.BGRID });
        }
    }
}
