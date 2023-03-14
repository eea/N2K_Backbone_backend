using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Species : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        [Key]
        public long id { get; set; }
        public string SiteCode { get; set; }
        public int Version { get; set; }
        public string SpecieCode { get; set; }
        public int? PopulationMin { get; set; }
        public int? PopulationMax { get; set; }
        public string? Group { get; set; }
        public Boolean? SensitiveInfo { get; set; }
        public string? Resident { get; set; }
        public string? Breeding { get; set; }
        public string? Winter { get; set; }
        public string? Staging { get; set; }
        public string? Path { get; set; }
        public string? AbundaceCategory { get; set; }
        public string? Motivation { get; set; }
        public string? PopulationType { get; set; }
        public string? CountingUnit { get; set; }
        public string? Population { get; set; }
        public string? Insolation { get; set; }
        public string? Conservation { get; set; }
        public string? Global { get; set; }
        public Boolean? NonPersistence { get; set; }
        public string? DataQuality { get; set; }
        public string? SpecieType { get; set; }

        private readonly string dbConnection = "";
        public Species() { }

        public Species(string db)
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
            SqlParameter param2 = new SqlParameter("@SiteCode", this.SiteCode);
            SqlParameter param3 = new SqlParameter("@Version", this.Version);
            SqlParameter param4 = new SqlParameter("@SpecieCode", this.SpecieCode);
            SqlParameter param5 = new SqlParameter("@PopulationMin", this.PopulationMin);
            SqlParameter param6 = new SqlParameter("@PopulationMax", this.PopulationMax);
            SqlParameter param7 = new SqlParameter("@Group", this.Group);
            SqlParameter param8 = new SqlParameter("@SensitiveInfo", this.SensitiveInfo);
            SqlParameter param9 = new SqlParameter("@Resident", this.Resident);
            SqlParameter param10 = new SqlParameter("@Breeding", this.Breeding);
            SqlParameter param11 = new SqlParameter("@Winter", this.Winter);
            SqlParameter param12 = new SqlParameter("@Staging", this.Staging);
            SqlParameter param13 = new SqlParameter("@Path", this.Path);
            SqlParameter param14 = new SqlParameter("@AbundaceCategory", this.AbundaceCategory);
            SqlParameter param15 = new SqlParameter("@Motivation", this.Motivation);
            SqlParameter param16 = new SqlParameter("@PopulationType", this.PopulationType);
            SqlParameter param17 = new SqlParameter("@CountingUnit", this.CountingUnit);
            SqlParameter param18 = new SqlParameter("@Population", this.Population);
            SqlParameter param19 = new SqlParameter("@Insolation", this.Insolation);
            SqlParameter param20 = new SqlParameter("@Conservation", this.Conservation);
            SqlParameter param21 = new SqlParameter("@Global", this.Global);
            SqlParameter param22 = new SqlParameter("@NonPersistence", this.NonPersistence);
            SqlParameter param23 = new SqlParameter("@DataQuality", this.DataQuality);
            SqlParameter param24 = new SqlParameter("@SpecieType", this.SpecieType);


            cmd.CommandText = "INSERT INTO [Species] (  " +
               " [Id] ,[SiteCode],[Version] ,[SpecieCode],[PopulationMin],[PopulationMax],[Group],[SensitiveInfo],[Resident],[Breeding],[Winter],[Staging,[Path],[AbundaceCategory],[Motivation],[PopulationType],[CountingUnit],[Population] ,[Insolation] ,[Conservation],[Global],[NonPersistence] ,[DataQuality],[SpecieType] " +
                " VALUES (@Id, @SiteCode,@Version,@SpecieCode,@PopulationMin,@PopulationMax,@Group,@SensitiveInfo,@Resident,@Breeding,@Winter,@Staging,@Path,@AbundaceCategory,@Motivation,@PopulationType,@CountingUnit,@Population,@Insolation,@Conservation,@Global,@NonPersistence,@DataQuality, @SpecieType) ";

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
            cmd.Parameters.Add(param17);
            cmd.Parameters.Add(param18);
            cmd.Parameters.Add(param19);
            cmd.Parameters.Add(param20);
            cmd.Parameters.Add(param21);
            cmd.Parameters.Add(param22);
            cmd.Parameters.Add(param23);
            cmd.Parameters.Add(param24);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();
        }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Species>()
                .ToTable("Species")
                .HasKey(c => new { c.id });
        }
    }
}
