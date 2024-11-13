using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly string parent = "ExtractionFiles";

        public ExtractionService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

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
                Row r = new();
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
            public string? Spatial_Area_Decrease;
            public string? Spatial_Area_Increase;
            public string? SDF_Area_Difference;

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
                Row r = new();
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
            public string? Spatial_area_deleted;
            public string? Spatial_area_added;
            public string? Spatial_former_area;
            public string? Spatial_current_area;
            public string? SDF_former_area;
            public string? SDF_current_area;
            public string? SDF_area_difference;

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
                Row r = new();
                r.Append(CreateCell(BioRegions));
                r.Append(CreateCell(SiteCode));
                r.Append(CreateCell(Name));
                r.Append(CreateCell(SiteType));
                r.Append(CreateCell(Spatial_area_deleted));
                r.Append(CreateCell(Spatial_area_added));
                r.Append(CreateCell(Spatial_former_area));
                r.Append(CreateCell(Spatial_current_area));
                r.Append(CreateCell(SDF_former_area));
                r.Append(CreateCell(SDF_current_area));
                r.Append(CreateCell(SDF_area_difference));
                return r;
            }
        }

        public async Task<string> GetLast()
        {
            try
            {
                DirectoryInfo files = new(parent);
                FileInfo? latest = files.GetFiles("*.zip").OrderBy(f => f.CreationTime).LastOrDefault();
                return latest?.Name.Replace(".zip", "") ?? "";
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - GetLast", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task UpdateExtraction()
        {
            try
            {
                //string dir = Path.Combine(parent, DateTime.Now.ToString().Replace('/', '-').Replace(':', '-').Replace(' ', '_'));
                //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Updating extractions", "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                //string archive = await GenerateExcelFiles(dir);

                HttpClient client = new();
                String serverUrl = String.Format(_appSettings.Value.fme_service_extractions, "all-MS", _appSettings.Value.fme_security_token);
                try
                {
                    //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                    client.Timeout = TimeSpan.FromHours(5);
                    Task<HttpResponseMessage> response = client.GetAsync(serverUrl);
                    string content = await response.Result.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                }
                finally
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End Excel extraction generation"), "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                    client.Dispose();
                    //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString().ToString(), "End");
                }

                // delete files and folders from previous extractions
                DeleteFiles(await GetLast());
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - UpdateExtractions", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async void DeleteFiles(string dir, bool deleteAll = false)
        {
            try
            {
                List<string> folders = Directory.EnumerateDirectories(parent).ToList();
                folders.Where(d => !d.Contains(dir) || deleteAll).ToList().ForEach(d => Directory.Delete(d, true));
                List<string> files = Directory.EnumerateFiles(parent).ToList();
                files.Where(d => !d.Contains(dir) || deleteAll).ToList().ForEach(d => File.Delete(d));
            }
            catch (IOException iex)
            {
                // ommit
                await SystemLog.WriteAsync(SystemLog.errorLevel.Warning, iex, "ExtractionService - DeleteFiles Deleting non-empty directory", "", _dataContext.Database.GetConnectionString());
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - DeleteFiles", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<string> GenerateExcelFiles(string dir)
        {
            try
            {
                DirectoryInfo path = Directory.CreateDirectory(dir);
                List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>()
                    .Where(e => e.Status == Enumerations.HarvestingStatus.Harvested).ToListAsync();
                List<string> fileList = new();
                foreach(ProcessedEnvelopes e in envelopes)
                {
                    fileList.Add(await ExcelCountry(dir, e.Country, e.Version));
                }
                string result = Path.Combine(dir + ".zip");
                using (var archive = ZipArchive.Create())
                {
                    fileList.ForEach(f => archive.AddEntry(f, f));
                    archive.SaveTo(result, CompressionType.Deflate);
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - GenerateExcelFiles", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<string> ExcelCountry(string dir, string country, int version)
        {
            try
            {
                List<ExtChanges> allChangesBySiteCode = await GetData<ExtChanges>(Extractions.AllChangesBySiteCode, country, version);
                List<ExtChanges> allChangesByChanges = await GetData<ExtChanges>(Extractions.AllChangesByChanges, country, version);
                List<ExtSpatialChanges> allSpatialChanges = await GetData<ExtSpatialChanges>(Extractions.SpatialChanges, country, version);
                List<ExtAreaChanges> allAreaChanges = await GetData<ExtAreaChanges>(Extractions.AreaChanges, country, version);

                // write data to excel
                string fileName = Path.Combine(dir, country + ".xlsx");
                using (SpreadsheetDocument doc = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = doc.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    Sheets sheets = workbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    // All Changes By SiteCode
                    WorksheetPart worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet1 = new();
                    Row header1 = CreateHeader<ExtChanges>();
                    Columns cols1 = ColSize(header1);
                    workSheet1.Append(cols1);
                    workSheet1.Append(await InsertData<ExtChanges>(header1, allChangesBySiteCode));
                    worksheetPart1.Worksheet = workSheet1;
                    Sheet sheet1 = new() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart1), SheetId = 1, Name = "All Changes By SiteCode" };
                    sheets.Append(sheet1);

                    // All Changes By Changes
                    WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet2 = new();
                    Row header2 = CreateHeader<ExtChanges>();
                    Columns cols2 = ColSize(header2);
                    workSheet2.Append(cols2);
                    workSheet2.Append(await InsertData<ExtChanges>(header2, allChangesByChanges));
                    worksheetPart2.Worksheet = workSheet2;
                    Sheet sheet2 = new() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart2), SheetId = 2, Name = "All Changes By Changes" };
                    sheets.Append(sheet2);

                    // Spatial Changes
                    WorksheetPart worksheetPart3 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet3 = new();
                    Row header3 = CreateHeader<ExtSpatialChanges>();
                    Columns cols3 = ColSize(header3);
                    workSheet3.Append(cols3);
                    workSheet3.Append(await InsertData<ExtSpatialChanges>(header3, allSpatialChanges));
                    worksheetPart3.Worksheet = workSheet3;
                    worksheetPart3.Worksheet.Save();
                    Sheet sheet3 = new() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart3), SheetId = 3, Name = "Spatial Changes" };
                    sheets.Append(sheet3);

                    // Area Changes
                    WorksheetPart worksheetPart4 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet4 = new();
                    Row header4 = CreateHeader<ExtAreaChanges>();
                    Columns cols4 = ColSize(header4);
                    workSheet4.Append(cols4);
                    workSheet4.Append(await InsertData<ExtAreaChanges>(header4, allAreaChanges));
                    worksheetPart4.Worksheet = workSheet4;
                    worksheetPart4.Worksheet.Save();
                    Sheet sheet4 = new() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart4), SheetId = 4, Name = "Area Changes" };
                    sheets.Append(sheet4);

                }
                return fileName;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - ExcelCountry: Contry: " + country + " - Version: " + version, "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<List<T>> GetData<T>(string query, string country, int version)
        {
            try
            {
                List<T> result = new();
                using (SqlConnection con = new(_dataContext.Database.GetConnectionString()))
                {
                    con.Open();
                    SqlCommand cmd = new(query, con);
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
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - GetData: Contry: " + country + " - Version: " + version, "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<SheetData> InsertData<T>(Row header, List<T> data) where T : ISqlResult
        {
            try
            {
                SheetData sheetData = new();
                sheetData.Append(header);
                data.ForEach(c => sheetData.Append(c.ToRow()));
                return sheetData;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ExtractionService - InsertData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private Row CreateHeader<T>() where T : ISqlResult
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
            Row header = new();
            headerNames.ForEach(h => header.Append(new Cell { CellValue = new CellValue(h), DataType = CellValues.String }));
            return header;
        }

        private Columns ColSize(Row header)
        {
            Columns columns = new();
            List<Cell> cells = header.Elements<Cell>().ToList();
            foreach (Cell c in cells)
            {
                // https://stackoverflow.com/questions/18268620/openxml-auto-size-column-width-in-excel#26180406
                double width = Math.Truncate(((double)(c.CellValue?.InnerText.Length ?? 0) * 12 + 30) / 12 * 256) / 256;
                Column col = new()
                {
                    BestFit = true,
                    Min = (UInt32)cells.IndexOf(c) + 1,
                    Max = (UInt32)cells.IndexOf(c) + 1,
                    CustomWidth = true,
                    Width = (DoubleValue)width
                };
                columns.Append(col);
            }
            return columns;
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
                return new ExtAreaChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_area_deleted = r["Spatial area deleted (ha)"].ToString(),
                    Spatial_area_added = r["Spatial area added (ha)"].ToString(),
                    Spatial_former_area = r["Spatial former area (ha)"].ToString(),
                    Spatial_current_area = r["Spatial current area (ha)"].ToString(),
                    SDF_former_area = r["SDF former area (ha)"].ToString(),
                    SDF_current_area = r["SDF current area (ha)"].ToString(),
                    SDF_area_difference = r["SDF area difference (ha)"].ToString(),
                };
            }
            else if (typeof(T) == typeof(ExtSpatialChanges))
            {
                return new ExtSpatialChanges
                {
                    BioRegions = r["BioRegions"].ToString(),
                    SiteCode = r["SiteCode"].ToString(),
                    Name = r["Name"].ToString(),
                    SiteType = r["SiteType"].ToString(),
                    Spatial_Area_Decrease = r["Spatial Area Decrease"].ToString(),
                    Spatial_Area_Increase = r["Spatial Area Increase"].ToString(),
                    SDF_Area_Difference = r["SDF Area Difference"].ToString(),
                };
            }
            else
            {
                throw new Exception("Type missmatch");
            }
        }
    }
}