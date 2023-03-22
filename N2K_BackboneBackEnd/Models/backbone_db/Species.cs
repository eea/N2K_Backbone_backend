using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Species : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        [Key]
        public long Id { get; set; }
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

        private string dbConnection = "";
        public Species() { }

        public Species(string db)
        {
            dbConnection = db;
        }


        public void SaveRecord(string db)
        {
            try 
            {   
                this.dbConnection = db;
                SqlConnection conn = null;
                SqlCommand cmd = null;

                conn = new SqlConnection(this.dbConnection);
                conn.Open();
                cmd = conn.CreateCommand();
                SqlParameter param1 = new SqlParameter("@SiteCode", this.SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", this.Version);
                SqlParameter param3 = new SqlParameter("@SpecieCode", this.SpecieCode);
                SqlParameter param4 = new SqlParameter("@PopulationMin", this.PopulationMin is null ? DBNull.Value : this.PopulationMin);
                SqlParameter param5 = new SqlParameter("@PopulationMax", this.PopulationMax is null ? DBNull.Value : this.PopulationMax);
                SqlParameter param6 = new SqlParameter("@Group", this.Group is null ? DBNull.Value : this.Group);
                SqlParameter param7 = new SqlParameter("@SensitiveInfo", this.SensitiveInfo is null ? DBNull.Value : this.SensitiveInfo);
                SqlParameter param8 = new SqlParameter("@Resident", this.Resident is null ? DBNull.Value : this.Resident);
                SqlParameter param9 = new SqlParameter("@Breeding", this.Breeding is null ? DBNull.Value : this.Breeding);
                SqlParameter param10 = new SqlParameter("@Winter", this.Winter is null ? DBNull.Value : this.Winter);
                SqlParameter param11 = new SqlParameter("@Staging", this.Staging is null ? DBNull.Value : this.Staging);
                SqlParameter param12 = new SqlParameter("@Path", this.Path is null ? DBNull.Value : this.Path);
                SqlParameter param13 = new SqlParameter("@AbundaceCategory", this.AbundaceCategory is null ? DBNull.Value : this.AbundaceCategory);
                SqlParameter param14 = new SqlParameter("@Motivation", this.Motivation is null ? DBNull.Value : this.Motivation);
                SqlParameter param15 = new SqlParameter("@PopulationType", this.PopulationType is null ? DBNull.Value : this.PopulationType);
                SqlParameter param16 = new SqlParameter("@CountingUnit", this.CountingUnit is null ? DBNull.Value : this.CountingUnit);
                SqlParameter param17 = new SqlParameter("@Population", this.Population is null ? DBNull.Value : this.Population);
                SqlParameter param18 = new SqlParameter("@Insolation", this.Insolation is null ? DBNull.Value : this.Insolation);
                SqlParameter param19 = new SqlParameter("@Conservation", this.Conservation is null ? DBNull.Value : this.Conservation);
                SqlParameter param20 = new SqlParameter("@Global", this.Global is null ? DBNull.Value : this.Global);
                SqlParameter param21 = new SqlParameter("@NonPersistence", this.NonPersistence is null ? DBNull.Value : this.NonPersistence);
                SqlParameter param22 = new SqlParameter("@DataQuality", this.DataQuality is null ? DBNull.Value : this.DataQuality);
                SqlParameter param23 = new SqlParameter("@SpecieType", this.SpecieType is null ? DBNull.Value : this.SpecieType);


                cmd.CommandText = "INSERT INTO [Species] (  " +
                    "[SiteCode],[Version],[SpecieCode],[PopulationMin],[PopulationMax],[Group],[SensitiveInfo],[Resident],[Breeding],[Winter],[Staging],[Path],[AbundaceCategory],[Motivation],[PopulationType],[CountingUnit],[Population],[Insolation],[Conservation],[Global],[NonPersistence],[DataQuality],[SpecieType]) " +
                    " VALUES (@SiteCode,@Version,@SpecieCode,@PopulationMin,@PopulationMax,@Group,@SensitiveInfo,@Resident,@Breeding,@Winter,@Staging,@Path,@AbundaceCategory,@Motivation,@PopulationType,@CountingUnit,@Population,@Insolation,@Conservation,@Global,@NonPersistence,@DataQuality,@SpecieType) ";

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

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Species - SaveRecord", "");
            }
        }

        public async static Task<int> SaveBulkRecord(string db, List<Species> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Species";
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Species>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Species - SaveBulkRecord", "");
                return 0;
            }
        }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Species>()
                .ToTable("Species")
                .HasKey(c => new { c.Id });
        }
    }
}
