using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Respondents : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        [Key]
        public long ID { get; }
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? locatorName { get; set; }
        public string? addressArea { get; set; }
        public string? postName { get; set; }
        public string? postCode { get; set; }
        public string? thoroughfare { get; set; }
        public string? addressUnstructured { get; set; }
        public string? name { get; set; }
        public string? Email { get; set; }
        public string? AdminUnit { get; set; }
        public string? LocatorDesignator { get; set; }


        private string dbConnection = "";

        public Respondents () { }

        public Respondents(string db) {
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
            SqlParameter param2 = new SqlParameter("@Version", this.Version);
            SqlParameter param3 = new SqlParameter("@locatorName", this.locatorName is null ? DBNull.Value : this.locatorName);
            SqlParameter param4 = new SqlParameter("@addressArea", this.addressArea is null ? DBNull.Value : this.addressArea);
            SqlParameter param5 = new SqlParameter("@postName", this.postName is null ? DBNull.Value : this.postName);
            SqlParameter param6 = new SqlParameter("@postCode", this.postCode is null ? DBNull.Value : this.postCode);
            SqlParameter param7 = new SqlParameter("@thoroughfare", this.thoroughfare is null ? DBNull.Value : this.thoroughfare);
            SqlParameter param8 = new SqlParameter("@addressUnstructured", this.addressUnstructured is null ? DBNull.Value : this.addressUnstructured);
            SqlParameter param9 = new SqlParameter("@name", this.name is null ? DBNull.Value : this.name);
            SqlParameter param10 = new SqlParameter("@Email", this.Email is null ? DBNull.Value : this.Email);
            SqlParameter param11 = new SqlParameter("@AdminUnit", this.AdminUnit is null ? DBNull.Value : this.AdminUnit);
            SqlParameter param12 = new SqlParameter("@LocatorDesignator", this.LocatorDesignator is null ? DBNull.Value : this.LocatorDesignator);

            cmd.CommandText = "INSERT INTO [Respondents] (  " +
                "[SiteCode], [Version],[locatorName],[addressArea],[postName],[postCode],[thoroughfare],[addressUnstructured] ,[name] ,[Email] ,[AdminUnit] ,[LocatorDesignator]) " +
                " VALUES (@SiteCode,@Version,@locatorName,@addressArea,@postName,@postCode,@thoroughfare,@addressUnstructured,@name,@Email,@AdminUnit,@LocatorDesignator) ";

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
            builder.Entity<Respondents>()
                .ToTable("Respondents")
                .HasKey("ID");

        }
       
    }
}
