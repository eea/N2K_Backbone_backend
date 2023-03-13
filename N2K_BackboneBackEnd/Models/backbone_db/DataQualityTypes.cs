using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class DataQualityTypes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string HabitatCode { get; set; }
        public string SpeciesCode { get; set; }


        public DataQualityTypes() { }

        public DataQualityTypes(string db)
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
            SqlParameter param1 = new SqlParameter("@Id", this.Id);
            SqlParameter param2 = new SqlParameter("@Name", this.Name);
            SqlParameter param3 = new SqlParameter("@HabitatCode", this.HabitatCode);
            SqlParameter param4 = new SqlParameter("@SpeciesCode", this.SpeciesCode);

            cmd.CommandText = "INSERT INTO [DataQualityTypes] (  " +
                "[Id],[Name],[HabitatCode] ,[SpeciesCode]" +
                " VALUES (@Id,@Name,@HabitatCode,@SpeciesCode";

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
            builder.Entity<DataQualityTypes>()
                .ToTable("DataQualityTypes")
                .HasKey(c => c.Id);
        }
       



    }
}
