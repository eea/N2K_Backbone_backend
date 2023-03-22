using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.Data;
using System.Data.Common;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    public class SiteChangeDb : IEntityModel, IEntityModelBackboneDB, IEntityModelBackboneDBHarvesting
    {

        [Key]
        public long ChangeId { get; set; }

        public string SiteCode { get; set; } = String.Empty;
        [NotMapped]
        public string SiteName { get; set; } = String.Empty;
        public int Version { get; set; }
        public string? Country { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }


        [NotMapped]
        public int NumChanges { get; set; }

        public string? NewValue { get; set; }
        public string? OldValue { get; set; }

        public string? Detail { get; set; }

        public string? Code { get; set; }
        public string? Section { get; set; }
        public int VersionReferenceId { get; set; }
        public string? FieldName { get; set; }
        public string ReferenceSiteCode { get; set; } = String.Empty;

        public int? N2KVersioningVersion { get; set; }

        [NotMapped]
        public bool? JustificationRequired { get; set; }
        [NotMapped]
        public bool? JustificationProvided { get; set; }

        [NotMapped]
        public bool? HasGeometry { get; set; }


        [NotMapped]
        public List<SiteChangeView> subRows { get; set; } = new List<SiteChangeView>();

        private string dbConnection = "";

        private static System.Data.DataTable PrepareDataForBulkCopy(IList<SiteChangeDb> data, SqlBulkCopy copy)
        {
            IList<string> notMappedFields = new List<string>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SiteChangeDb));
            System.Data.DataTable table = new System.Data.DataTable();
            //check if the field has a NotMapped attribute.
            //if so, do not include it in the output datatable
            foreach (PropertyDescriptor prop in properties)
            {
                var notMapped = false;
                foreach (var attr in prop.Attributes)
                {
                    if (attr.ToString().IndexOf("NotMappedAttribute") > -1)
                    {
                        notMapped = true;
                        notMappedFields.Add(prop.Name);
                        break;
                    }
                }
                if (!notMapped)
                {
                    if (prop.Name == "Level" || prop.Name == "Status")
                    {
                        table.Columns.Add(prop.Name, typeof(String));
                    }
                    else
                    {
                        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                    copy.ColumnMappings.Add(prop.Name, prop.Name);

                }
            }

            foreach (SiteChangeDb item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    if (!notMappedFields.Contains(prop.Name))
                    {
                        if (prop.Name == "Level" || prop.Name == "Status")
                        {
                            if (prop.GetValue(item) == DBNull.Value)
                            {
                                row[prop.Name] = DBNull.Value;
                            }
                            else
                                row[prop.Name] = prop.GetValue(item).ToString();
                        }
                        else
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
    

        public SiteChangeDb() { }

        public SiteChangeDb(string db)
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
                SqlParameter param3 = new SqlParameter("@Country", this.Country is null ? DBNull.Value : this.Country);
                SqlParameter param4 = new SqlParameter("@PopulationMin", this.Status is null ? DBNull.Value : this.Status);
                SqlParameter param5 = new SqlParameter("@PopulationMax", this.Tags is null ? DBNull.Value : this.Tags);
                SqlParameter param6 = new SqlParameter("@Group", this.Level is null ? DBNull.Value : this.Level);
                SqlParameter param7 = new SqlParameter("@SensitiveInfo", this.ChangeCategory is null ? DBNull.Value : this.ChangeCategory);
                SqlParameter param8 = new SqlParameter("@Resident", this.ChangeType is null ? DBNull.Value : this.ChangeType);
                SqlParameter param9 = new SqlParameter("@Breeding", this.NewValue is null ? DBNull.Value : this.NewValue);
                SqlParameter param10 = new SqlParameter("@Winter", this.OldValue is null ? DBNull.Value : this.OldValue);
                SqlParameter param11 = new SqlParameter("@Staging", this.Detail is null ? DBNull.Value : this.Detail);
                SqlParameter param12 = new SqlParameter("@Path", this.Code is null ? DBNull.Value : this.Code);
                SqlParameter param13 = new SqlParameter("@AbundaceCategory", this.Section is null ? DBNull.Value : this.Section);
                SqlParameter param14 = new SqlParameter("@Motivation", this.VersionReferenceId);
                SqlParameter param15 = new SqlParameter("@PopulationType", this.FieldName is null ? DBNull.Value : this.FieldName);
                SqlParameter param16 = new SqlParameter("@CountingUnit", this.ReferenceSiteCode is null ? DBNull.Value : this.ReferenceSiteCode);
                SqlParameter param17 = new SqlParameter("@Population", this.N2KVersioningVersion is null ? DBNull.Value : this.N2KVersioningVersion);


                cmd.CommandText = "INSERT INTO [Changes] (  " +
                    "[SiteCode],[Version],[Country],[Status],[Tags] ,[Level],[ChangeCategory],[ChangeType],[NewValue],[OldValue] ,[Detail],[Code],[Section],[VersionReferenceId],[FieldName],[ReferenceSiteCode],[N2KVersioningVersion]) " +
                    " VALUES (@SiteCode,@Version,@Country,@Status,@Tags,@Level,@ChangeCategory,@ChangeType,@NewValue,@OldValue,@Detail,@Code,@Section,@VersionReferenceId,@FieldName,@ReferenceSiteCode,@N2KVersioningVersion) ";

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

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangeDb - SaveRecord", "");
            }
        }

        public static async Task<int> SaveBulkRecord(string db, List<SiteChangeDb> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    using (var copy = new SqlBulkCopy(db))
                    {
                        copy.DestinationTableName = "Changes";
                        DataTable data = PrepareDataForBulkCopy(listData, copy);
                        await copy.WriteToServerAsync(data);
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangeDb - SaveBulkRecord", "");
                return 0;
            }
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());


        }
    }


    public class SiteChangeDbNumsperLevel :  IEntityModel, IEntityModelBackboneDB
    {

        public long ChangeId { get; set; }

        public string SiteCode { get; set; } = String.Empty;
        public string SiteName { get; set; } = String.Empty;
        public int Version { get; set; }
        public string? Country { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

        [NotMapped]
        public int NumChanges { get; set; }

        public string? NewValue { get; set; }
        public string? OldValue { get; set; }

        public string? Detail { get; set; }

        public string? Code { get; set; }
        public string? Section { get; set; }
        public int VersionReferenceId { get; set; }
        public string? FieldName { get; set; }
        public string ReferenceSiteCode { get; set; } = String.Empty;

        public bool? HasGeometry { get; set; }


        public bool? JustificationRequired { get; set; }
        public bool? JustificationProvided { get; set; }


        public List<SiteChangeView> subRows { get; set; } = new List<SiteChangeView>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDbNumsperLevel>()
                .HasNoKey()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDbNumsperLevel>()
                .HasNoKey()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());

        }


    }

}
