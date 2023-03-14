using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

            cmd.CommandText = "INSERT INTO [Sites] (  " +
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


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HasNationalProtection>()
                .ToTable("HasNationalProtection")
                .HasKey(c => new { c.ID });
        }
    }
}
