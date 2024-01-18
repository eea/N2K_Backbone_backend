using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.Data;
using System.ComponentModel;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteChangeDb : IEntityModel, IEntityModelBackboneDB
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
        public LineageTypes? LineageChangeType { get; set; }
        [NotMapped]
        public String? AffectedSites { get; set; }
        [NotMapped]
        public int NumChanges { get; set; }
        public string? NewValue { get; set; }
        public string? OldValue { get; set; }
        public string? Detail { get; set; }
        public string? Code { get; set; }
        public string? Section { get; set; }
        public int VersionReferenceId { get; set; }
        public string? FieldName { get; set; }
        public string? ReferenceSiteCode { get; set; }
        public int? N2KVersioningVersion { get; set; }
        [NotMapped]
        public bool? JustificationRequired { get; set; }
        [NotMapped]
        public bool? JustificationProvided { get; set; }
        [NotMapped]
        public bool? HasGeometry { get; set; }
        [NotMapped]
        public List<SiteChangeView> subRows { get; set; } = new List<SiteChangeView>();

        private string dbConnection = string.Empty;

        private static System.Data.DataTable PrepareDataForBulkCopy(IList<SiteChangeDb> data, SqlBulkCopy copy)
        {
            IList<string> notMappedFields = new List<string>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SiteChangeDb));
            System.Data.DataTable table = new();
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

        public async static Task<int> SaveBulkRecord(string db, List<SiteChangeDb> listData)
        {
            try
            {
                if (listData.Count > 0)
                {
                    int num_tries = 0;
                    int max_tries = 10;
                    bool success = false;
                    //try the bulk save up to 10 times (in case it fires deadlock errors) 
                    while (!success && num_tries < max_tries)
                    {
                        //try the bul
                        try
                        {
                            using (var copy = new SqlBulkCopy(db))
                            {
                                copy.DestinationTableName = "Changes";
                                copy.BulkCopyTimeout = 3000;
                                DataTable data = PrepareDataForBulkCopy(listData, copy);
                                await copy.WriteToServerAsync(data);
                                success = true;
                            }
                        }
                        catch
                        {
                            //wait 2 seconds before attempting the next save bulk
                            await Task.Delay(2000);
                            num_tries++;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangeDb - SaveBulkRecord", "", db);
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

    public class SiteChangeDbNumsperLevel : IEntityModel, IEntityModelBackboneDB
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