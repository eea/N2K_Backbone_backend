using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace N2K_BackboneBackEnd.Services
{
    public class ExtractionService : IExtractionService
    {
        private interface ISqlResult { }

        private class ExtChanges : ISqlResult
        {
            public string? BioRegions;
            public string? SiteCode;
            public string? Name;
            public string? SiteType;
            public string? Level;
            public string? ChangeCategory;
            public string? ChangeType;
            public string? NewValue;
            public string? OldValue;
            public string? Code;

            internal Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? "")
                    };
                }
                Row r = new Row();
                r.Append(CreateCell(BioRegions ?? ""));
                r.Append(CreateCell(SiteCode ?? ""));
                r.Append(CreateCell(Name ?? ""));
                r.Append(CreateCell(SiteType ?? ""));
                r.Append(CreateCell(Level ?? ""));
                r.Append(CreateCell(ChangeCategory ?? ""));
                r.Append(CreateCell(ChangeType ?? ""));
                r.Append(CreateCell(NewValue ?? ""));
                r.Append(CreateCell(OldValue ?? ""));
                r.Append(CreateCell(Code ?? ""));
                return r;
            }
        }

        private class ExtSpatialChanges : ISqlResult
        {
            public string? BioRegions;
            public string? SiteCode;
            public string? Name;
            public string? SiteType;
            public double? Spatial_Area_Decrease;
            public double? Spatial_Area_Increase;
            public double? SDF_Area_Difference;

            internal Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? "")
                    };
                }
                Row r = new Row();
                r.Append(CreateCell(BioRegions));
                r.Append(CreateCell(SiteCode));
                r.Append(CreateCell(Name));
                r.Append(CreateCell(SiteType));
                r.Append(CreateCell(Spatial_Area_Decrease.ToString()));
                r.Append(CreateCell(Spatial_Area_Increase.ToString()));
                r.Append(CreateCell(SDF_Area_Difference.ToString()));
                return r;
            }
        }

        private class ExtAreaChanges : ISqlResult
        {
            public string? BioRegions;
            public string? SiteCode;
            public string? Name;
            public string? SiteType;
            public double? Spatial_area_deleted;
            public double? Spatial_area_added;
            public double? Spatial_former_area;
            public double? Spatial_current_area;
            public double? SDF_former_area;
            public double? SDF_current_area;
            public double? SDF_area_difference;

            internal Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? "")
                    };
                }
                Row r = new Row();
                r.Append(CreateCell(BioRegions));
                r.Append(CreateCell(SiteCode));
                r.Append(CreateCell(Name));
                r.Append(CreateCell(SiteType));
                r.Append(CreateCell(Spatial_area_deleted.ToString()));
                r.Append(CreateCell(Spatial_area_added.ToString()));
                r.Append(CreateCell(Spatial_former_area.ToString()));
                r.Append(CreateCell(Spatial_current_area.ToString()));
                r.Append(CreateCell(SDF_former_area.ToString()));
                r.Append(CreateCell(SDF_current_area.ToString()));
                r.Append(CreateCell(SDF_area_difference.ToString()));
                return r;
            }
        }

        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly N2KBackboneContext _dataContext;

        private Extractions ext = new();

        public ExtractionService(IOptions<ConfigSettings> app, N2KBackboneContext dataContext)
        {
            _appSettings = app;
            _dataContext = dataContext;
        }

        public Task<FileContentResult> DownloadExtractions()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> UpdateExtractions()
        {
            throw new NotImplementedException();
        }

        private async Task GenerateExcelFiles()
        {
            string parent = "ExtractionFiles";
            string dir = Path.Combine(parent, DateTime.Now.ToString().Replace('/', '_').Replace(':', '_'));
            try
            {
                DirectoryInfo path = Directory.CreateDirectory(dir);
                List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>()
                    .Where(e => e.Status == Enumerations.HarvestingStatus.Harvested).ToListAsync();
                List<string> fileList = new();
                envelopes.ForEach(async e =>
                    fileList.Add(await ExcelCountry(path.FullName, e.Country, e.Version))
                );
                using (var zip = File.OpenWrite(dir + ".zip"))
                using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
                {
                    foreach (var filePath in fileList)
                    {
                        zipWriter.Write(filePath, filePath);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<string> ExcelCountry(string dir, string country, int version)
        {
            using (SqlConnection con = (SqlConnection)_dataContext.Database.GetDbConnection())
            {
                List<ExtChanges> allChangesBySiteCode = await GetData<ExtChanges>(con, Extractions.AllChangesBySiteCode, country, version);
                List<ExtChanges> allChangesByChanges = await GetData<ExtChanges>(con, Extractions.AllChangesByChanges, country, version);
                List<ExtSpatialChanges> allSpatialChanges = await GetData<ExtSpatialChanges>(con, Extractions.SpatialChanges, country, version);
                List<ExtAreaChanges> allAreaChanges = await GetData<ExtAreaChanges>(con, Extractions.AreaChanges, country, version);

                // write data to excel
                string fileName = Path.Combine(dir, country + ".xlsx");
                using (SpreadsheetDocument doc = SpreadsheetDocument.Create(Path.Combine(dir, country + ".xlsx"), SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = doc.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                    Sheet sheet1 = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Changes By SiteCode" };
                    sheets.Append(sheet1);
                    allChangesBySiteCode.ForEach(c => sheet1.Append(c.ToRow()));

                    Sheet sheet2 = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 2, Name = "Changes By Changes" };
                    sheets.Append(sheet2);
                    allChangesByChanges.ForEach(c => sheet2.Append(c.ToRow()));

                    Sheet sheet3 = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 3, Name = "Spatial Changes" };
                    sheets.Append(sheet3);
                    allSpatialChanges.ForEach(c => sheet3.Append(c.ToRow()));

                    Sheet sheet4 = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 4, Name = "Area Changes" };
                    sheets.Append(sheet4);
                    allAreaChanges.ForEach(c => sheet4.Append(c.ToRow()));

                }
                return fileName;
            }
        }

        private async Task<List<T>> GetData<T>(SqlConnection con, string query, string country, int version) where T : ISqlResult
        {
            List<T> result = new();
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@COUNTRYCODE", country);
            cmd.Parameters.AddWithValue("@COUNTRYVERSION", version);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add((T)DataMapper<T>(reader));
                }
            }
            return result;
        }

        private ISqlResult DataMapper<T>(SqlDataReader r) where T : ISqlResult
        {
            T check = default(T);
            if (check is ExtChanges)
            {
                return new ExtChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Level = r["Level"].ToString(),
                    ChangeCategory = r["ChangeCategory"].ToString(),
                    ChangeType = r["ChangeType"].ToString(),
                    NewValue = r["NewValue"].ToString(),
                    OldValue = r["OldValue"].ToString(),
                    Code = r["Code"].ToString(),
                };
            }
            else if (check is ExtAreaChanges)
            {
                return new ExtAreaChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_area_deleted = (double)r["Spatial area deleted"],
                    Spatial_area_added = (double)r["Spatial area added"],
                    Spatial_former_area = (double)r["Spatial former area"],
                    Spatial_current_area = (double)r["Spatial current area"],
                    SDF_former_area = (double)r["SDF former area"],
                    SDF_current_area = (double)r["SDF current area"],
                    SDF_area_difference = (double)r["SDF area difference"],
                };
            }
            else if (check is ExtSpatialChanges)
            {
                return new ExtSpatialChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_Area_Decrease = (double)r["Spatial Area Decrease"],
                    Spatial_Area_Increase = (double)r["Spatial Area Increase"],
                    SDF_Area_Difference = (double)r["SDF Area Difference"],
                };
            }
            else
            {
                throw new Exception("Type missmatch");
            }
        }

    }
}

