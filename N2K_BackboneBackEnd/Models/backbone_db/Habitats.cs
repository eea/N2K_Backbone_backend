using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Habitats : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
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


        private string dbConnection = "";

        public Habitats() { }

        public Habitats(string db)
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
                SqlParameter param3 = new SqlParameter("@HabitatCode", this.HabitatCode);
                SqlParameter param4 = new SqlParameter("@CoverHA", this.CoverHA is null ? DBNull.Value : this.CoverHA);
                SqlParameter param5 = new SqlParameter("@PriorityForm", this.PriorityForm is null ? DBNull.Value : this.PriorityForm);
                SqlParameter param6 = new SqlParameter("@Representativity", this.Representativity is null ? DBNull.Value : this.Representativity);
                SqlParameter param7 = new SqlParameter("@DataQty", this.DataQty is null ? DBNull.Value : this.DataQty);
                SqlParameter param8 = new SqlParameter("@Conservation", this.Conservation is null ? DBNull.Value : this.Conservation);
                SqlParameter param9 = new SqlParameter("@GlobalAssesments", this.GlobalAssesments is null ? DBNull.Value : this.GlobalAssesments);
                SqlParameter param10 = new SqlParameter("@RelativeSurface", this.RelativeSurface is null ? DBNull.Value : this.RelativeSurface);
                SqlParameter param11 = new SqlParameter("@Percentage", this.Percentage is null ? DBNull.Value : this.Percentage);
                SqlParameter param12 = new SqlParameter("@ConsStatus", this.ConsStatus is null ? DBNull.Value : this.ConsStatus);
                SqlParameter param13 = new SqlParameter("@Caves", this.Caves is null ? DBNull.Value : this.Caves);
                SqlParameter param14 = new SqlParameter("@PF", this.PF is null ? DBNull.Value : this.PF);
                SqlParameter param15 = new SqlParameter("@NonPresenciInSite", this.NonPresenciInSite is null ? DBNull.Value : this.NonPresenciInSite);

                cmd.CommandText = "INSERT INTO [Habitats] (  " +
                    "[SiteCode],[Version],[HabitatCode],[CoverHA],[PriorityForm],[Representativity],[DataQty],[Conservation],[GlobalAssesments],[RelativeSurface],[Percentage],[ConsStatus],[Caves],[PF],[NonPresenciInSite]) " +
                    " VALUES (@SiteCode,@Version,@HabitatCode,@CoverHA,@PriorityForm,@Representativity,@DataQty,@Conservation,@GlobalAssesments,@RelativeSurface,@Percentage,@ConsStatus,@Caves,@PF,@NonPresenciInSite) ";

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

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "Habitats - SaveRecord", "");
                }
    }
        public async static Task<int> SaveBulkRecord(string db, List<Habitats> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Habitats";
                        copy.BulkCopyTimeout = 3000;
                        DataTable data = TypeConverters.PrepareDataForBulkCopy<Habitats>(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Habitats - SaveBulkRecord", "");
                return 0;
            }
        }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats")
                .HasKey(c => new { c.id });
        }
    }
}
