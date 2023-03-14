using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DetailedProtectionStatus : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string? SiteCode { get; set; }
        public int? Version { get; set; }
        public string? DesignationCode { get; set; }
        public string? Name { get; set; }
        public long ID { get; set; }
        public string? OverlapCode { get; set; }
        public decimal? OverlapPercentage { get; set; }
        public string? Convention { get; set; }

        private string dbConnection = "";

        public DetailedProtectionStatus() { }

        public DetailedProtectionStatus(string db)
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
            SqlParameter param3 = new SqlParameter("@DesignationCode", this.DesignationCode);
            SqlParameter param4 = new SqlParameter("@Name", this.Name);
            SqlParameter param5 = new SqlParameter("@OverlapCode", this.OverlapCode);
            SqlParameter param6 = new SqlParameter("@OverlapPercentage", this.OverlapPercentage);
            SqlParameter param7 = new SqlParameter("@Convention", this.Convention);

            cmd.CommandText = "INSERT INTO [DetailedProtectionStatus] (  " +
                "[SiteCode],[Version],[DesignationCode],[Name],[OverlapCode],[OverlapPercentage],[Convention]) " +
                " VALUES (@SiteCode,@Version,@DesignationCode,@Name,@OverlapCode,@OverlapPercentage,@Convention) ";

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


        public static void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<DetailedProtectionStatus>()
                .Property(b => b.OverlapPercentage)
                .HasPrecision(38, 2);

            builder.Entity<DetailedProtectionStatus>()
                .ToTable("DetailedProtectionStatus")
                .HasKey(c => new { c.ID });
        }
    }
}
