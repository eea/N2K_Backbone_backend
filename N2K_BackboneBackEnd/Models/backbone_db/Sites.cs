using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Sites : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public Boolean? Current { get; set; }
        public string? Name { get; set; }
        public DateTime? CompilationDate { get; set; }
        public DateTime? ModifyTS { get; set; }
        public SiteChangeStatus? CurrentStatus { get; set; }
        public string? CountryCode { get; set; }
        public string? SiteType { get; set; }
        public double? AltitudeMin { get; set; }
        public double? AltitudeMax { get; set; }
        public int? N2KVersioningVersion { get; set; }
        public int? N2KVersioningRef { get; set; }
        public decimal? Area { get; set; }
        public decimal? Length { get; set; }
        public Boolean? JustificationRequired { get; set; }
        public Boolean? JustificationProvided { get; set; }
        public DateTime? DateConfSCI { get; set; }
        public Boolean? Priority { get; set; }
        public DateTime? DatePropSCI { get; set; }
        public DateTime? DateSpa { get; set; }
        public DateTime? DateSac { get; set; }

        private string dbConnection = "";

        public Sites() { }

        public Sites(string db)
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
                SqlParameter param3 = new SqlParameter("@Current", this.Current is null ? DBNull.Value : this.Current);
                SqlParameter param4 = new SqlParameter("@Name", this.Name is null ? DBNull.Value : this.Name);
                SqlParameter param5 = new SqlParameter("@CompilationDate", this.CompilationDate is null ? DBNull.Value : this.CompilationDate);
                SqlParameter param6 = new SqlParameter("@ModifyTS", this.ModifyTS is null ? DBNull.Value : this.ModifyTS);
                SqlParameter param7 = new SqlParameter("@CurrentStatus", this.CurrentStatus is null ? DBNull.Value : this.CurrentStatus);
                SqlParameter param8 = new SqlParameter("@CountryCode", this.CountryCode is null ? DBNull.Value : this.CountryCode);
                SqlParameter param9 = new SqlParameter("@SiteType", this.SiteType is null ? DBNull.Value : this.SiteType);
                SqlParameter param10 = new SqlParameter("@AltitudeMin", this.AltitudeMin is null ? DBNull.Value : this.AltitudeMin);
                SqlParameter param11 = new SqlParameter("@AltitudeMax", this.AltitudeMax is null ? DBNull.Value : this.AltitudeMax);
                SqlParameter param12 = new SqlParameter("@N2KVersioningVersion", this.N2KVersioningVersion is null ? DBNull.Value : this.N2KVersioningVersion);
                SqlParameter param13 = new SqlParameter("@N2KVersioningRef", this.N2KVersioningRef is null ? DBNull.Value : this.N2KVersioningRef);
                SqlParameter param14 = new SqlParameter("@Area", this.Area is null ? DBNull.Value : this.Area);
                SqlParameter param15 = new SqlParameter("@Length", this.Length is null ? DBNull.Value : this.Length);
                SqlParameter param16 = new SqlParameter("@JustificationRequired", this.JustificationRequired is null ? DBNull.Value : this.JustificationRequired);
                SqlParameter param17 = new SqlParameter("@JustificationProvided", this.JustificationProvided is null ? DBNull.Value : this.JustificationProvided);
                SqlParameter param18 = new SqlParameter("@DateConfSCI", this.DateConfSCI is null ? DBNull.Value : this.DateConfSCI);
                SqlParameter param19 = new SqlParameter("@Priority", this.Priority is null ? DBNull.Value : this.Priority);
                SqlParameter param20 = new SqlParameter("@DatePropSCI", this.DatePropSCI is null ? DBNull.Value : this.DatePropSCI);
                SqlParameter param21 = new SqlParameter("@DateSpa", this.DateSpa is null ? DBNull.Value : this.DateSpa);
                SqlParameter param22 = new SqlParameter("@DateSac", this.DateSac is null ? DBNull.Value : this.DateSac);

                cmd.CommandText = "INSERT INTO [Sites] (  " +
                    "[SiteCode],[Version],[Current],[Name],[CompilationDate],[ModifyTS],[CurrentStatus],[CountryCode],[SiteType],[AltitudeMin],[AltitudeMax],[N2KVersioningVersion],[N2KVersioningRef],[Area],[Length],[JustificationRequired],[JustificationProvided],[DateConfSCI],[Priority],[DatePropSCI],[DateSpa],[DateSac]) " +
                    " VALUES (@SiteCode,@Version,@Current,@Name,@CompilationDate,@ModifyTS,@CurrentStatus,@CountryCode,@SiteType,@AltitudeMin,@AltitudeMax,@N2KVersioningVersion,@N2KVersioningRef,@Area,@Length,@JustificationRequired,@JustificationProvided,@DateConfSCI,@Priority,@DatePropSCI,@DateSpa,@DateSac) ";

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

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Sites - SaveRecord", "");
            }
        }

        public async static Task<int> SaveBulkRecord(string db, List<Sites> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Sites";
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Sites>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Sites - SaveBulkRecord", "");
                return 0;
            }
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sites>()
                .ToTable("Sites")
                .HasKey(c => new { c.SiteCode, c.Version });
        }
    }
}
