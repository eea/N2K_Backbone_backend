using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

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
            try
            {
                this.dbConnection = db;
                SqlConnection conn = null;
                SqlCommand cmd = null;

                conn = new SqlConnection(this.dbConnection);
                conn.Open();
                cmd = conn.CreateCommand();
                SqlParameter param1 = new SqlParameter("@SiteCode", this.SiteCode is null ? DBNull.Value : this.SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", this.Version);
                SqlParameter param3 = new SqlParameter("@ActivityCode", this.ActivityCode is null ? DBNull.Value : this.ActivityCode);
                SqlParameter param4 = new SqlParameter("@InOut", this.InOut is null ? DBNull.Value : this.InOut);
                SqlParameter param5 = new SqlParameter("@Intensity", this.Intensity is null ? DBNull.Value : this.Intensity);
                SqlParameter param6 = new SqlParameter("@PercentageAff", this.PercentageAff is null ? DBNull.Value : this.PercentageAff);
                SqlParameter param7 = new SqlParameter("@Influence", this.Influence is null ? DBNull.Value : this.Influence);
                SqlParameter param8 = new SqlParameter("@StartDate", this.StartDate is null ? DBNull.Value : this.StartDate);
                SqlParameter param9 = new SqlParameter("@EndDate", this.EndDate is null ? DBNull.Value : this.EndDate);
                SqlParameter param10 = new SqlParameter("@PollutionCode", this.PollutionCode is null ? DBNull.Value : this.PollutionCode);
                SqlParameter param11 = new SqlParameter("@Ocurrence", this.Ocurrence is null ? DBNull.Value : this.Ocurrence);
                SqlParameter param12 = new SqlParameter("@ImpactType", this.ImpactType is null ? DBNull.Value : this.ImpactType);

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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "IsImpactedBy - SaveRecord", "");
            }
        }

        public async static Task<int> SaveBulkRecord(string db, List<IsImpactedBy> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "IsImpactedBy";
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<IsImpactedBy>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "IsImpactedBy - SaveBulkRecord", "");
                return 0;
            }
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IsImpactedBy>()
                .ToTable("IsImpactedBy")
                .HasKey(c => new { c.Id });
        }
    }
}
