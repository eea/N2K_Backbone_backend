using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class NutsBySite : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string NutId { get; set; } = string.Empty;
        public double? CoverPercentage { get; set; }

        private string dbConnection = "";

        public NutsBySite() { }

        public NutsBySite(string db)
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
            SqlParameter param3 = new SqlParameter("@NutId", this.NutId);
            SqlParameter param4 = new SqlParameter("@CoverPercentage", this.CoverPercentage is null ? DBNull.Value : this.CoverPercentage);

            cmd.CommandText = "INSERT INTO [NutsBySite] (  " +
                "[SiteCode],[Version],[NutId],[CoverPercentage]) " +
                " VALUES (@SiteCode,@Version,@NutId,@CoverPercentage) ";

            cmd.Parameters.Add(param1);
            cmd.Parameters.Add(param2);
            cmd.Parameters.Add(param3);
            cmd.Parameters.Add(param4);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NutsBySite>()
                .ToTable("NutsBySite")
                .HasKey(c => new { c.SiteCode, c.Version, c.NutId });
        }
    }
}
