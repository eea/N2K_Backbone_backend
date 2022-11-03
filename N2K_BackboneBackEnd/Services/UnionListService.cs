using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Resources;
using Microsoft.Build.Execution;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO.Compression;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class UnionListService : IUnionListService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private const string ulBioRegSites = "ulBioRegSites";

        public UnionListService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

        public async Task<List<BioRegionTypes>> GetUnionBioRegionTypes()
        {
            return await _dataContext.Set<BioRegionTypes>().AsNoTracking().Where(bio => bio.BioRegionShortCode != null).ToListAsync();
        }

        public async Task<List<UnionListHeader>> GetUnionListHeadersByBioRegion(string? bioRegionShortCode)
        {
            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            List<UnionListHeader> unionListHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();

            return unionListHeaders;
        }

        public async Task<List<UnionListDetail>> GetCurrentSitesUnionListDetailByBioRegion(string? bioRegionShortCode)
        {
            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            List<UnionListDetail> unionListDetails = await _dataContext.Set<UnionListDetail>().FromSqlRaw($"exec dbo.spGetCurrentSitesUnionListDetailByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();

            return unionListDetails;
        }

        public async Task<List<UnionListHeader>> GetUnionListHeadersById(long? id)
        {
            return await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).ToListAsync();
        }


        private async Task<List<BioRegionSiteCode>> GetBioregionSiteCodesInUnionListComparer(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            string listName = string.Format("{0}_{1}_{2}_{3}_{4}", GlobalData.Username, ulBioRegSites, idSource, idTarget, string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions.Replace(",", "_"));
            List<BioRegionSiteCode> resultCodes = new List<BioRegionSiteCode>();
            if (cache.TryGetValue(listName, out List<BioRegionSiteCode> cachedList))
            {
                resultCodes = cachedList;
            }
            else
            {
                SqlParameter param1 = new SqlParameter("@idULHeaderSource", idSource);
                SqlParameter param2 = new SqlParameter("@idULHeaderTarget", idTarget);
                SqlParameter param3 = new SqlParameter("@bioRegions", string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions);

                resultCodes = await _dataContext.Set<BioRegionSiteCode>().FromSqlRaw($"exec dbo.spGetBioregionSiteCodesInUnionListComparer  @idULHeaderSource, @idULHeaderTarget, @bioRegions",
                                param1, param2, param3).ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(40000);
                cache.Set(listName, resultCodes, cacheEntryOptions);
            }
            return resultCodes;

        }

        public async Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            UnionListComparerSummaryViewModel res = new UnionListComparerSummaryViewModel();
            List<BioRegionSiteCode> resultCodes = await GetBioregionSiteCodesInUnionListComparer(idSource, idTarget, bioRegions, cache);
            res.BioRegSiteCodes = resultCodes.ToList();

            //Get the number of site codes per bio region
            List<BioRegionTypes> ulBioRegions = await GetUnionBioRegionTypes();

            var codesGrouped = resultCodes.GroupBy(n => n.BioRegion)
                         .Select(n => new UnionListComparerBioReg
                         {
                             BioRegion = n.Key,
                             Count = n.Count()
                         }).ToList();
            var _bioRegionSummary =
                (
                from p in ulBioRegions
                join co in codesGrouped on p.BioRegionShortCode equals co.BioRegion into PersonasColegio
                from pco in PersonasColegio.DefaultIfEmpty(new UnionListComparerBioReg { BioRegion = p.BioRegionShortCode, Count = 0 })
                select new UnionListComparerBioReg
                {
                    BioRegion = pco.BioRegion,
                    Count = pco.Count
                }).OrderBy(b => b.BioRegion).ToList();

            res.BioRegionSummary = _bioRegionSummary;
            return res;
        }


        public async Task<List<UnionListComparerDetailedViewModel>> CompareUnionLists(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache, int page = 1, int pageLimit = 0)
        {
            List<BioRegionSiteCode> ulSites = await GetBioregionSiteCodesInUnionListComparer(idSource, idTarget, bioRegions, cache);
            var startRow = (page - 1) * pageLimit;
            if (pageLimit > 0)
            {
                ulSites = ulSites
                    .Skip(startRow)
                    .Take(pageLimit)
                    .ToList();
            }

            //get the bioReg-SiteCodes of the source UL
            var _ulDetails = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idSource).ToListAsync();
            List<UnionListDetail> ulDetailsSource = (from src1 in ulSites
                                                     from trgt1 in _ulDetails.Where(trg1 => (src1.SiteCode == trg1.SCI_code) && (src1.BioRegion == trg1.BioRegion))
                                                     select trgt1
            ).ToList();
            _ulDetails.Clear();

            //get the bioReg-SiteCodes of the target UL
            _ulDetails = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idTarget).ToListAsync();
            List<UnionListDetail> ulDetailsTarget = (from src1 in ulSites
                                                     from trgt2 in _ulDetails.Where(trg2 => (src1.SiteCode == trg2.SCI_code) && (src1.BioRegion == trg2.BioRegion))
                                                     select trgt2
            ).ToList();

            //clear the memory
            _ulDetails.Clear();
            ulSites.Clear();

            List<UnionListComparerDetailedViewModel> result = new List<UnionListComparerDetailedViewModel>();
            //Changed
            var changedSites = (from source1 in ulDetailsSource
                                join target1 in ulDetailsTarget
                                     on new { source1.SCI_code, source1.BioRegion } equals new { target1.SCI_code, target1.BioRegion }
                                where source1.SCI_Name != target1.SCI_Name || source1.SCI_Name != target1.SCI_Name
                                 || source1.Priority != target1.Priority || source1.Area != target1.Area
                                 || source1.Length != target1.Length || source1.Lat != target1.Lat
                                 || source1.Long != target1.Long
                                select new { source1, target1 }).ToList();

            foreach (var item in changedSites)
            {
                UnionListComparerDetailedViewModel changedItem = new UnionListComparerDetailedViewModel();
                changedItem.BioRegion = item.source1.BioRegion;
                changedItem.Sitecode = item.source1.SCI_code;

                changedItem.SiteName = new UnionListValues<string>
                {
                    Source = item.source1.SCI_Name,
                    Target = item.target1.SCI_Name
                };


                changedItem.Priority = new UnionListValues<bool>
                {
                    Source = item.source1.Priority,
                    Target = item.target1.Priority
                };


                changedItem.Area = new UnionListValues<double>
                {
                    Source = item.source1.Area,
                    Target = item.target1.Area
                };

                changedItem.Length = new UnionListValues<double>
                {
                    Source = item.source1.Length,
                    Target = item.target1.Length
                };

                changedItem.Longitude = new UnionListValues<double>
                {
                    Source = item.source1.Long,
                    Target = item.target1.Long
                };

                changedItem.Latitude = new UnionListValues<double>
                {
                    Source = item.source1.Lat,
                    Target = item.target1.Lat
                };


                //COMPARE THE VALUES FIELD BY FIELD
                if ((string?)changedItem.SiteName.Source != (string?)changedItem.SiteName.Target)
                    changedItem.SiteName.Change = "SITENAME Changed";


                if ((bool?)changedItem.Priority.Source != (bool?)changedItem.Priority.Target)
                {
                    bool prioSource = ((bool?)changedItem.Priority.Source).HasValue ? ((bool?)changedItem.Priority.Source).Value : false;
                    bool prioTarget = ((bool?)changedItem.Priority.Target).HasValue ? ((bool?)changedItem.Priority.Target).Value : false;

                    if (prioSource && !prioTarget)
                    {
                        changedItem.Priority.Change = "PRIORITY_LOST";
                    }
                    else if (!prioSource == false && prioTarget)
                    {
                        changedItem.Priority.Change = "PRIORITY_GAIN";

                    }
                    else
                    {
                        changedItem.Priority.Change = "PRIORITY_CHANGED";
                    }
                }


                if ((double?)changedItem.Area.Source != (double?)changedItem.Area.Target)
                {
                    double source = ((double?)changedItem.Area.Source).HasValue ? ((double?)changedItem.Area.Source).Value : 0.0;
                    double target = ((double?)changedItem.Area.Target).HasValue ? ((double?)changedItem.Area.Target).Value : 0.0;

                    if (source < target)
                    {
                        changedItem.Area.Change = "AREA_INCREASED";
                    }
                    else if (source > target)
                    {
                        changedItem.Area.Change = "AREA_DECREASED";
                    }
                    else
                    {
                        changedItem.Area.Change = "AREA_CHANGED";
                    }
                }

                if ((double?)changedItem.Length.Source != (double?)changedItem.Length.Target)
                {
                    double source = ((double?)changedItem.Length.Source).HasValue ? ((double?)changedItem.Length.Source).Value : 0.0;
                    double target = ((double?)changedItem.Length.Target).HasValue ? ((double?)changedItem.Length.Target).Value : 0.0;

                    if (source < target)
                    {
                        changedItem.Length.Change = "LENGTH_INCREASED";
                    }
                    else if (source > target)
                    {
                        changedItem.Length.Change = "LENGTH_DECREASED";
                    }
                    else
                    {
                        changedItem.Length.Change = "LENGTH_CHANGED";
                    }
                }

                if ((double?)changedItem.Latitude.Source != (double?)changedItem.Latitude.Target)
                {
                    changedItem.Latitude.Change = "LATITUDE_CHANGED";
                }

                if ((double?)changedItem.Longitude.Source != (double?)changedItem.Longitude.Target)
                {
                    changedItem.Longitude.Change = "LONGITUDE_CHANGED";
                }

                changedItem.Changes = "ATTRIBUTES CHANGED";
                result.Add(changedItem);
            }


            //Added in source
            var sourceOnlySites = (from source2 in ulDetailsSource
                                   join target2 in ulDetailsTarget on new { source2.SCI_code, source2.BioRegion } equals new { target2.SCI_code, target2.BioRegion } into t
                                   from od in t.DefaultIfEmpty()
                                   where od == null
                                   select source2).ToList();
            foreach (var item in sourceOnlySites)
            {
                UnionListComparerDetailedViewModel changedItem = new UnionListComparerDetailedViewModel();
                changedItem.BioRegion = item.BioRegion;
                changedItem.Sitecode = item.SCI_code;

                changedItem.SiteName = new UnionListValues<string>
                {
                    Source = item.SCI_Name,
                    Target = null
                };


                changedItem.Area = new UnionListValues<double>
                {
                    Source = item.Area,
                    Target = null
                };

                changedItem.Length = new UnionListValues<double>
                {
                    Source = item.Length,
                    Target = null
                };

                changedItem.Latitude = new UnionListValues<double>
                {
                    Source = item.Lat,
                    Target = null
                };


                changedItem.Longitude = new UnionListValues<double>
                {
                    Source = item.Long,
                    Target = null
                };

                changedItem.Changes = "ADDED";
                result.Add(changedItem);
            }


            //Deleted in source            
            var targetOnlySites = (from target3 in ulDetailsTarget
                                   join source3 in ulDetailsSource on new { target3.SCI_code, target3.BioRegion } equals new { source3.SCI_code, source3.BioRegion } into t
                                   from od in t.DefaultIfEmpty()
                                   where od == null
                                   select target3).ToList();
            foreach (var item in targetOnlySites)
            {
                UnionListComparerDetailedViewModel changedItem = new UnionListComparerDetailedViewModel();
                changedItem.BioRegion = item.BioRegion;
                changedItem.Sitecode = item.SCI_code;

                changedItem.SiteName = new UnionListValues<string>
                {
                    Target = item.SCI_Name,
                    Source = null
                };


                changedItem.Area = new UnionListValues<double>
                {
                    Target = item.Area,
                    Source = null
                };

                changedItem.Length = new UnionListValues<double>
                {
                    Target = item.Length,
                    Source = null
                };

                changedItem.Latitude = new UnionListValues<double>
                {
                    Target = item.Lat,
                    Source = null
                };


                changedItem.Longitude = new UnionListValues<double>
                {
                    Target = item.Long,
                    Source = null
                };
                changedItem.Changes = "DELETED";
                result.Add(changedItem);
            }
            return result.OrderBy(a => a.BioRegion).ThenBy(b => b.Sitecode).ToList();
        }

        public async Task<List<UnionListHeader>> CreateUnionList(string name, Boolean final)
        {
            SqlParameter param1 = new SqlParameter("@name", name);
            SqlParameter param2 = new SqlParameter("@creator", GlobalData.Username);
            SqlParameter param3 = new SqlParameter("@final", final);

            await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final ", param1, param2, param3);
            return await GetUnionListHeadersByBioRegion(null);
        }

        public async Task<List<UnionListHeader>> UpdateUnionList(long id, string name, Boolean final)
        {
            UnionListHeader unionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).FirstOrDefaultAsync();
            if (unionList != null)
            {
                if (name != "string")
                    unionList.Name = name;

                unionList.Final = final;
                unionList.UpdatedBy = GlobalData.Username;
                unionList.UpdatedDate = DateTime.Now;

                _dataContext.Set<UnionListHeader>().Update(unionList);
            }
            await _dataContext.SaveChangesAsync();

            return await GetUnionListHeadersByBioRegion(null);


        }

        public async Task<int> DeleteUnionList(long id)
        {
            int result = 0;
            UnionListHeader? unionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().FirstOrDefaultAsync(ulh => ulh.idULHeader == id);
            if (unionList != null)
            {
                _dataContext.Set<UnionListHeader>().Remove(unionList);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;
        }

        public async Task<string> UnionListDownload(string bioregs)
        {
            IAttachedFileHandler? fileHandler = null;
            var username = GlobalData.Username;
            if (_appSettings.Value.AttachedFiles.AzureBlob)
            {
                fileHandler = new AzureBlobHandler(_appSettings.Value.AttachedFiles);
            }
            else
            {
                fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles);
            }

            string repositoryPath = Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Value.AttachedFiles.JustificationFolder);
            string tempZipFile = repositoryPath + "//" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + GlobalData.Username.Split("@")[0] + "_Union List.zip";
            
            //Delete file to avoid duplicates with the same name
            string[] files = Directory.GetFiles(repositoryPath);
            foreach (string file in files)
            {
                if (file.EndsWith("_Union List.zip"))
                    File.Delete(file);
            }
            await fileHandler.DeleteUnionListsFilesAsync();

            //Create a new zip file
            ZipArchive archive = ZipFile.Open(tempZipFile, ZipArchiveMode.Create);

            string[] bioRegions = bioregs.Split(',');
            foreach (string bioRegion in bioRegions)
            {
                //The file path must be parametrized in the web.config
                string filePath = repositoryPath + "//" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + bioRegion + "_Union List.xlsx";
                //Create the Excel document
                SpreadsheetDocument workbook = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                //Create the different sections and parts necesaries for the Excel
                WorkbookPart workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                WorksheetPart sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);
                Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);
                //Just a sheet for the excel book
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = bioRegion }; //Page name = BioRegion name
                sheets.Append(sheet);

                #region Retrive the data to insert
                UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

                SqlParameter param1 = new SqlParameter("@idHeader", currentUnionList.idULHeader);
                SqlParameter param2 = new SqlParameter("@bioregion", bioRegion);
                List<UnionListDetailExcel> currentDetails = await _dataContext.Set<UnionListDetailExcel>().FromSqlRaw("exec dbo.spGetCurrentUnionListDetailByHeaderIdAndBioRegion  @idHeader, @bioregion ", param1, param2).AsNoTracking().ToListAsync();
                #endregion

                #region Styling
                WorkbookStylesPart stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet(
                    new Fonts(
                        new Font(                                                           // Index 0 - The default font.
                            new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 },
                            new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                            new FontName() { Val = "Calibri" })
                    ),
                    new Fills(
                        new Fill(                                                           // Index 0 - The default fill.
                            new PatternFill() { PatternType = PatternValues.None }),
                        new Fill(                                                           // Index 1 - The grey fill.
                            new PatternFill(
                                new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "C0C0C0" } }
                            )
                            { PatternType = PatternValues.Solid }
                        )
                    ),
                    new Borders(
                        new Border(                                                         // Index 0 - The default border.
                            new LeftBorder(),
                            new RightBorder(),
                            new TopBorder(),
                            new BottomBorder(),
                            new DiagonalBorder()),
                        new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                            new LeftBorder(
                                new Color() { Auto = true }
                            )
                            { Style = BorderStyleValues.Thin },
                            new RightBorder(
                                new Color() { Auto = true }
                            )
                            { Style = BorderStyleValues.Thin },
                            new TopBorder(
                                new Color() { Auto = true }
                            )
                            { Style = BorderStyleValues.Thin },
                            new BottomBorder(
                                new Color() { Auto = true }
                            )
                            { Style = BorderStyleValues.Thin }
                        )
                    ),
                    new CellFormats(
                        new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Bottom })
                        { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true },   // Index 0 - Left align. The default cell style.  If a cell does not have a style index applied it will use this style combination instead

                        new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Bottom })
                        { FontId = 0, FillId = 0, BorderId = 0, ApplyFont = true },   // Index 1 - Right align

                        new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Bottom })
                        { FontId = 0, FillId = 1, BorderId = 1, ApplyFont = true }    // Index 2 - Header
                    )
                );
                stylesPart.Stylesheet.Save();
                #endregion

                Row row = new Row();
                Cell cell = new Cell();

                #region Header of the columns, but we can handwrite it because we know the structure
                //SCI code
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("SCI code");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Name of SCI
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Name of SCI");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Priority
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Priority");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Area of SCI (ha)
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Area of SCI (ha)");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Length of SCI (km)
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Length of SCI (km)");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Longitude
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Longitude");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                //Latitude
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue("Latitude");
                cell.StyleIndex = 2;
                row.AppendChild(cell);
                cell = new Cell();
                #endregion

                sheetData.AppendChild(row);
                row = new Row();
                foreach (UnionListDetailExcel ulde in currentDetails)
                {
                    #region Content row creation
                    row = new Row();
                    //In the same way we know the structure of the data, so we can call for each field
                    //SCI code
                    cell = new Cell();
                    cell.DataType = CellValues.String; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue(ulde.SCI_code); //The GetString is because SqlDataReader. With the Entity it's not necesary
                    cell.StyleIndex = 0;
                    row.AppendChild(cell);
                    //Name of SCI
                    cell = new Cell();
                    cell.DataType = CellValues.String; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue(ulde.SCI_Name);
                    cell.StyleIndex = 0;
                    row.AppendChild(cell);
                    //Priority
                    cell = new Cell();
                    cell.DataType = CellValues.String; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue(ulde.Priority);
                    cell.StyleIndex = 0;
                    row.AppendChild(cell);
                    //Area of SCI (ha)
                    cell = new Cell();
                    cell.DataType = CellValues.Number; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue((double)ulde.Area);
                    cell.StyleIndex = 1;
                    row.AppendChild(cell);
                    //Length of SCI (km)
                    cell = new Cell();
                    cell.DataType = CellValues.Number; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue((double)ulde.Length);
                    cell.StyleIndex = 1;
                    row.AppendChild(cell);
                    //Longitude
                    cell = new Cell();
                    cell.DataType = CellValues.Number; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue((double)ulde.Long);
                    cell.StyleIndex = 1;
                    row.AppendChild(cell);
                    //Latitude
                    cell = new Cell();
                    cell.DataType = CellValues.Number; //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                    cell.CellValue = new CellValue((double)ulde.Lat);
                    cell.StyleIndex = 1;
                    row.AppendChild(cell);
                    #endregion

                    sheetData.AppendChild(row);
                }

                workbookPart.Workbook.Save();
                workbook.Close();

                ZipArchiveEntry fileInZip = archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                File.Delete(filePath);
            }

            archive.Dispose();
            List<String> url = await fileHandler.UploadFileAsync(tempZipFile);

            return url[0];
        }

        public async Task<UnionListComparerSummaryViewModel> GetUnionListComparerSummary(IMemoryCache _cache)
        {
            //Delete Current if it already exists
            UnionListHeader? tempUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().FirstOrDefaultAsync(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby));
            if (tempUnionList != null)
            {
                _dataContext.Set<UnionListHeader>().Remove(tempUnionList);
                await _dataContext.SaveChangesAsync();
            }

            //Get latest release
            UnionListHeader? latestUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name != _appSettings.Value.current_ul_name) && (ulh.CreatedBy != _appSettings.Value.current_ul_createdby) && (ulh.Final == true)).OrderByDescending(ulh => ulh.Date).FirstOrDefaultAsync();

            //Create Current
            SqlParameter param1 = new SqlParameter("@name", _appSettings.Value.current_ul_name);
            SqlParameter param2 = new SqlParameter("@creator", _appSettings.Value.current_ul_createdby);
            SqlParameter param3 = new SqlParameter("@final", false);
            await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final ", param1, param2, param3);

            //Get Current
            UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

            return await GetCompareSummary(latestUnionList.idULHeader, currentUnionList.idULHeader, null, _cache);
        }

        public async Task<List<UnionListComparerDetailedViewModel>> GetUnionListComparer(IMemoryCache _cache, int page = 1, int pageLimit = 0)
        {
            //Get latest release
            UnionListHeader? latestUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name != _appSettings.Value.current_ul_name) && (ulh.CreatedBy != _appSettings.Value.current_ul_createdby) && (ulh.Final == true)).OrderByDescending(ulh => ulh.Date).FirstOrDefaultAsync();

            //Create Current
            SqlParameter param1 = new SqlParameter("@name", _appSettings.Value.current_ul_name);
            SqlParameter param2 = new SqlParameter("@creator", _appSettings.Value.current_ul_createdby);
            SqlParameter param3 = new SqlParameter("@final", false);
            await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final ", param1, param2, param3);

            //Get Current
            UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

            List<UnionListComparerDetailedViewModel> ulSites = await CompareUnionLists(latestUnionList.idULHeader, currentUnionList.idULHeader, null, _cache, page, pageLimit);

            return ulSites;
        }

    }
}
