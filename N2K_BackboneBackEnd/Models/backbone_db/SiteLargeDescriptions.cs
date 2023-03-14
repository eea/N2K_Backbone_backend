using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteLargeDescriptions : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Quality { get; set; }
        public string? Vulnarab { get; set; }
        public string? Designation { get; set; }
        public string? ManagPlan { get; set; }
        public string? Documentation { get; set; }
        public string? OtherCharact { get; set; }
        public string? ManagConservMeasures { get; set; }
        public string? ManagPlanUrl { get; set; }
        public string? ManagStatus { get; set; }

        private readonly string dbConnection = "";

        public SiteLargeDescriptions() { }

        public SiteLargeDescriptions(string db)
        {
            dbConnection = db;
        }


        public void SaveRecord()
        {
            //string dbConnection = db;
            SqlConnection conn = null;
            SqlCommand cmd = null;

            conn = new SqlConnection(this.dbConnection);
            conn.Open();
            cmd = conn.CreateCommand();
            SqlParameter param1 = new SqlParameter("@SiteCode", this.SiteCode);
            SqlParameter param2 = new SqlParameter("@Version", this.Version);
            SqlParameter param3 = new SqlParameter("@Quality", this.Quality);
            SqlParameter param4 = new SqlParameter("@Vulnarab", this.Vulnarab);
            SqlParameter param5 = new SqlParameter("@Designation", this.Designation);
            SqlParameter param6 = new SqlParameter("@ManagPlan", this.ManagPlan);
            SqlParameter param7 = new SqlParameter("@Documentation", this.Documentation);
            SqlParameter param8 = new SqlParameter("@OtherCharact", this.OtherCharact);
            SqlParameter param9 = new SqlParameter("@ManagConservMeasures", this.ManagConservMeasures);
            SqlParameter param10 = new SqlParameter("@ManagPlanUrl", this.ManagPlanUrl);
            SqlParameter param11 = new SqlParameter("@ManagStatus", this.ManagStatus);

            cmd.CommandText = "INSERT INTO [SiteLargeDescriptions] (  " +
                "[SiteCode],[Version],[Quality],[Vulnarab],[Designation],[ManagPlan],[Documentation],[OtherCharact],[ManagConservMeasures],[ManagPlanUrl],[ManagStatus]) " +
                " VALUES (@SiteCode,@Version,@Quality,@Vulnarab,@Designation,@ManagPlan,@Documentation,@OtherCharact,@ManagConservMeasures,@ManagPlanUrl,@ManagStatus) ";

            cmd.Parameters.Add(param1);
            cmd.Parameters.Add(param2);
            cmd.Parameters.Add(param3);
            cmd.Parameters.Add(param4);
            cmd.Parameters.Add(param5);
            cmd.Parameters.Add(param6);
            cmd.Parameters.Add(param7);
            cmd.Parameters.Add(param8);
            cmd.Parameters.Add(param9);
            cmd.Parameters.Add(param10);
            cmd.Parameters.Add(param11);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteLargeDescriptions>()
                .ToTable("SiteLargeDescriptions")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}
