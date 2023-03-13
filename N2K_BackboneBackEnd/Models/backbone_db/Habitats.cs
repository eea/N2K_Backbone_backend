using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB
    {
        public long id { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string HabitatCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? CoverHA { get; set; }
        public Boolean? PriorityForm { get; set; }
        public string? Representativity { get; set; }
        public int? DataQty { get; set; }
        public string? Conservation { get; set; }
        public string? GlobalAssesments { get; set; }
        public string? RelativeSurface { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Percentage { get; set; }
        public string? ConsStatus { get; set; }
        public string? Caves { get; set; }
        public string? PF { get; set; }
        public int? NonPresenciInSite { get; set; }


        private readonly string dbConnection = "";

        public Habitats() { }

        public Habitats(string db)
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
            SqlParameter param1 = new SqlParameter("@id", this.id);
            SqlParameter param2 = new SqlParameter("@SiteCode", this.SiteCode);
            SqlParameter param3 = new SqlParameter("@Version", this.Version);
            SqlParameter param4 = new SqlParameter("@HabitatCode", this.HabitatCode);
            SqlParameter param5 = new SqlParameter("@CoverHA", this.CoverHA);
            SqlParameter param6 = new SqlParameter("@PriorityForm", this.PriorityForm);
            SqlParameter param7 = new SqlParameter("@Representativity", this.Representativity);
            SqlParameter param8 = new SqlParameter("@DataQty", this.DataQty);
            SqlParameter param9 = new SqlParameter("@Conservation", this.Conservation);
            SqlParameter param10 = new SqlParameter("@GlobalAssesments", this.GlobalAssesments);
            SqlParameter param11 = new SqlParameter("@RelativeSurface", this.RelativeSurface);
            SqlParameter param12 = new SqlParameter("@Percentage", this.Percentage);
            SqlParameter param13 = new SqlParameter("@ConsStatus", this.ConsStatus);
            SqlParameter param14 = new SqlParameter("@Caves", this.Caves);
            SqlParameter param15 = new SqlParameter("@PF", this.PF);
            SqlParameter param16 = new SqlParameter("@NonPresenciInSite", this.NonPresenciInSite);

            cmd.CommandText = "INSERT INTO [Habitats] (  " +
                "[id],[SiteCode],[Version],[HabitatCode],[CoverHA],[PriorityForm],[Representativity],[DataQty],[Conservation],[GlobalAssesments],[RelativeSurface],[Percentage],[ConsStatus],[Caves],[PF],[NonPresenciInSite]" +
                " VALUES (@id,@SiteCode,@Version,@HabitatCode,@CoverHA,@PriorityForm,@Representativity,@DataQty,@Conservation,@GlobalAssesments,@RelativeSurface,@Percentage,@ConsStatus,@Caves,@PF,@NonPresenciInSite ";

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
            cmd.Parameters.Add(param13);
            cmd.Parameters.Add(param14);
            cmd.Parameters.Add(param15);
            cmd.Parameters.Add(param16);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();
        }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.id });
        }
    }
}
