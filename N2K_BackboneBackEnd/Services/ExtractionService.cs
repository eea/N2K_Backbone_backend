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

namespace N2K_BackboneBackEnd.Services
{
    public class ExtractionService : IExtractionService
    {
        private interface ISqlResult
        {
            public Row ToRow();
        }

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

            public Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? ""),
                        DataType = CellValues.String
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

            public Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? ""),
                        DataType = CellValues.String
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

            public Row ToRow()
            {
                Cell CreateCell(string? val)
                {
                    return new Cell
                    {
                        CellValue = new CellValue(val ?? ""),
                        DataType = CellValues.String
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

        public Task<FileContentResult> DownloadExtraction()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateExtraction()
        {
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Updating extractions", "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                await GenerateExcelFiles();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task GenerateExcelFiles()
        {
            string parent = "ExtractionFiles";
            string dir = Path.Combine(parent, DateTime.Now.ToString().Replace('/', '_').Replace(':', '_'));
            DirectoryInfo path = Directory.CreateDirectory(dir);
            List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>()
                .Where(e => e.Status == Enumerations.HarvestingStatus.Harvested).ToListAsync();
            List<string> fileList = new();
            envelopes.ForEach(async e =>
                fileList.Add(await ExcelCountry(Path.Combine(parent, path.Name), e.Country, e.Version))
            );
            using (var archive = ZipArchive.Create())
            {
                fileList.ForEach(f => archive.AddEntry(f, f));
                archive.SaveTo(Path.Combine(dir + ".zip"), CompressionType.Deflate);
            }
        }

        private async Task<string> ExcelCountry(string dir, string country, int version)
        {
            List<ExtChanges> allChangesBySiteCode = GetData<ExtChanges>(Extractions.AllChangesBySiteCode, country, version);
            Console.WriteLine(allChangesBySiteCode.Count());
            List<ExtChanges> allChangesByChanges = GetData<ExtChanges>(Extractions.AllChangesByChanges, country, version);
            List<ExtSpatialChanges> allSpatialChanges = GetData<ExtSpatialChanges>(Extractions.SpatialChanges, country, version);
            List<ExtAreaChanges> allAreaChanges = GetData<ExtAreaChanges>(Extractions.AreaChanges, country, version);


            // write data to excel
            string fileName = Path.Combine(dir, country + ".xlsx");
            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = doc.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                Sheets sheets = workbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                // All Changes By SiteCode
                WorksheetPart worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                Worksheet workSheet1 = new Worksheet();
                workSheet1.Append(InsertData<ExtChanges>(allChangesBySiteCode));
                worksheetPart1.Worksheet = workSheet1;
                worksheetPart1.Worksheet.Save();
                Sheet sheet1 = new Sheet() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart1), SheetId = 1, Name = "All Changes By SiteCode" };
                sheets.Append(sheet1);

                // All Changes By Changes
                WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                Worksheet workSheet2 = new Worksheet();
                Row header2Changes = new Row();
                workSheet1.Append(header2Changes);
                workSheet2.Append(InsertData<ExtChanges>(allChangesByChanges));
                worksheetPart2.Worksheet = workSheet2;
                worksheetPart2.Worksheet.Save();
                Sheet sheet2 = new Sheet() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart2), SheetId = 2, Name = "All Changes By Changes" };
                sheets.Append(sheet2);

                // Spatial Changes
                WorksheetPart worksheetPart3 = workbookPart.AddNewPart<WorksheetPart>();
                Worksheet workSheet3 = new Worksheet();
                workSheet3.Append(InsertData<ExtSpatialChanges>(allSpatialChanges));
                worksheetPart3.Worksheet = workSheet3;
                worksheetPart3.Worksheet.Save();
                Sheet sheet3 = new Sheet() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart3), SheetId = 3, Name = "Spatial Changes" };
                sheets.Append(sheet3);

                // Area Changes
                WorksheetPart worksheetPart4 = workbookPart.AddNewPart<WorksheetPart>();
                Worksheet workSheet4 = new Worksheet();
                workSheet4.Append(InsertData<ExtAreaChanges>(allAreaChanges));
                worksheetPart4.Worksheet = workSheet4;
                worksheetPart4.Worksheet.Save();
                Sheet sheet4 = new Sheet() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart4), SheetId = 4, Name = "Area Changes" };
                sheets.Append(sheet4);

            }
            return fileName;
        }

        private List<T> GetData<T>(string query, string country, int version)
        {
            List<T> result = new();
            using (SqlConnection con = new(_dataContext.Database.GetConnectionString()))
            {
                con.Open();
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
            }
            return result;
        }

        private SheetData InsertData<T>(List<T> data) where T : ISqlResult
        {
            List<string> headerNames = (new Func<List<string>>(() =>
            {
                if (typeof(T) == typeof(ExtChanges))
                {
                    return new List<string> { "BioRegions", "SiteCode", "Name", "SiteType", "Level", "ChangeCategory", "ChangeType", "NewValue", "OldValue", "Code" };
                }
                if (typeof(T) == typeof(ExtSpatialChanges))
                {
                    return new List<string> { "BioRegions", "SiteCode", "Name", "SiteType", "Spatial Area Decrease", "Spatial Area Increase", "SDF Area Difference" };
                }
                if (typeof(T) == typeof(ExtAreaChanges))
                {
                    return new List<string> { "BioRegions", "SiteCode", "Name", "SiteType", "Spatial area deleted (ha)", "Spatial area added (ha)", "Spatial former area (ha)", "Spatial current area (ha)", "SDF former area (ha)", "SDF current area (ha)", "SDF area difference (ha)" };
                }
                else
                {
                    throw new Exception("Invalid data type");
                }
            }))();
            Row header = new Row();
            headerNames.ForEach(h => header.Append(new Cell { CellValue = new CellValue(h), DataType = CellValues.String }));
            SheetData sheetData = new();
            sheetData.Append(header);
            data.ForEach(c => sheetData.Append(c.ToRow()));
            return sheetData;
        }

        private ISqlResult DataMapper<T>(SqlDataReader r)
        {
            if (typeof(T) == typeof(ExtChanges))
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
            else if (typeof(T) == typeof(ExtAreaChanges))
            {
                double areaDeleted = 0;
                Double.TryParse(r["Spatial area deleted (ha)"].ToString(), out areaDeleted);
                double areaAdded = 0;
                Double.TryParse(r["Spatial area added (ha)"].ToString(), out areaAdded);
                double formerArea = 0;
                Double.TryParse(r["Spatial former area (ha)"].ToString(), out formerArea);
                double currentArea = 0;
                Double.TryParse(r["Spatial current area (ha)"].ToString(), out currentArea);
                double sdfFormerArea = 0;
                Double.TryParse(r["SDF former area (ha)"].ToString(), out sdfFormerArea);
                double sdfCurrentArea = 0;
                Double.TryParse(r["SDF current area (ha)"].ToString(), out sdfCurrentArea);
                double areaDifference = 0;
                Double.TryParse(r["SDF area difference (ha)"].ToString(), out areaDifference);
                return new ExtAreaChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_area_deleted = areaDeleted,
                    Spatial_area_added = areaAdded,
                    Spatial_former_area = formerArea,
                    Spatial_current_area = currentArea,
                    SDF_former_area = sdfFormerArea,
                    SDF_current_area = sdfCurrentArea,
                    SDF_area_difference = areaDifference,
                };
            }
            else if (typeof(T) == typeof(ExtSpatialChanges))
            {
                double areaDecrease = 0;
                Double.TryParse(r["Spatial Area Decrease"].ToString(), out areaDecrease);
                double areaIncrease = 0;
                Double.TryParse(r["Spatial Area Increase"].ToString(), out areaIncrease);
                double areaDiff = 0;
                Double.TryParse(r["SDF Area Difference"].ToString(), out areaDiff);
                return new ExtSpatialChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_Area_Decrease = areaDecrease,
                    Spatial_Area_Increase = areaIncrease,
                    SDF_Area_Difference = areaDiff,
                };
            }
            else
            {
                throw new Exception("Type missmatch");
            }
        }

    }
}

