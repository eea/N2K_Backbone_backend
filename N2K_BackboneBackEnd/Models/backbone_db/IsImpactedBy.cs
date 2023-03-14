using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class IsImpactedBy : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? ActivityCode { get; set; }
        public string? InOut { get; set; }
        public string? Intensity { get; set; }
        public double? PercentageAff { get; set; }
        public string? Influence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PollutionCode { get; set; }
        public string? Ocurrence { get; set; }
        public string? ImpactType { get; set; }
        public long Id { get; set; }

        private string dbConnection = "";

        public IsImpactedBy() { }

        public IsImpactedBy(string db)
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
            SqlParameter param3 = new SqlParameter("@ActivityCode", this.ActivityCode);
            SqlParameter param4 = new SqlParameter("@InOut", this.InOut);
            SqlParameter param5 = new SqlParameter("@Intensity", this.Intensity);
            SqlParameter param6 = new SqlParameter("@PercentageAff", this.PercentageAff);
            SqlParameter param7 = new SqlParameter("@Influence", this.Influence);
            SqlParameter param8 = new SqlParameter("@StartDate", this.StartDate);
            SqlParameter param9 = new SqlParameter("@EndDate", this.EndDate);
            SqlParameter param10 = new SqlParameter("@PollutionCode", this.PollutionCode);
            SqlParameter param11 = new SqlParameter("@Ocurrence", this.Ocurrence);
            SqlParameter param12 = new SqlParameter("@ImpactType", this.ImpactType);

            cmd.CommandText = "INSERT INTO [IsImpactedBy] (  " +
                "[SiteCode],[Version],[ActivityCode],[InOut],[Intensity],[PercentageAff],[Influence],[StartDate],[EndDate],[PollutionCode],[Ocurrence],[ImpactType]) " +
                " VALUES (@SiteCode,@Version,@ActivityCode,@InOut,@Intensity,@PercentageAff,@Influence,@StartDate,@EndDate,@PollutionCode,@Ocurrence,@ImpactType) ";

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
            cmd.Parameters.Add(param12);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .ToTable("IsImpactedBy")
                .HasKey(c => new { c.Id });
        }
    }
}
