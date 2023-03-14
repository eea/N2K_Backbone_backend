using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class ProcessedEnvelopes : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        [Key]
        public long Id { get; }

        public DateTime ImportDate { get;   set; }
        public string? Country { get;  set; }

        public int Version { get;   set; }

        public string? Importer { get; set; }

        public HarvestingStatus Status { get;  set; }
        
        public DateTime N2K_VersioningDate { get; set; }


        private string dbConnection = "";

        public ProcessedEnvelopes() { }

        public ProcessedEnvelopes(string db)
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
            SqlParameter param1 = new SqlParameter("@Country", this.Country);
            SqlParameter param2 = new SqlParameter("@Version", this.Version);
            SqlParameter param3 = new SqlParameter("@ImportDate", this.ImportDate);
            SqlParameter param4 = new SqlParameter("@Importer", this.Importer);
            SqlParameter param5 = new SqlParameter("@Id", this.Id);
            SqlParameter param6 = new SqlParameter("@Status", this.Status);
            SqlParameter param7 = new SqlParameter("@N2K_VersioningDate", this.N2K_VersioningDate);

            cmd.CommandText = "INSERT INTO [ProcessedEnvelopes] (  " +
                "[Country],[Version],[ImportDate],[Importer],[Id],[Status],[N2K_VersioningDate]) " +
                " VALUES (@Country,@Version,@ImportDate,@Importer,@Id,@Status,@N2K_VersioningDate) ";

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
            builder.Entity<ProcessedEnvelopes>()
                .ToTable("ProcessedEnvelopes")
                .HasKey("Id");

        }
       
    }
}
