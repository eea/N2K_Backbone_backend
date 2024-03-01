using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
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
            try
            {
                return await _dataContext.Set<BioRegionTypes>().AsNoTracking().Where(bio => bio.BioRegionShortCode != null).ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - GetUnionBioRegionTypes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<List<BioRegionSiteCode>> GetBioregionSiteCodesInUnionListComparer(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            try
            {
                string listName = string.Format("{0}_{1}_{2}_{3}", GlobalData.Username, ulBioRegSites, idSource, idTarget);
                List<BioRegionSiteCode> resultCodes = new();
                if (cache.TryGetValue(listName, out List<BioRegionSiteCode> cachedList))
                {
                    resultCodes = cachedList;
                    if (!string.IsNullOrEmpty(bioRegions))
                    {
                        List<string> bioRegList = bioRegions.Split(",").ToList();
                        resultCodes = (from rc in resultCodes
                                       join brl in bioRegList on rc.BioRegion equals brl
                                       select rc).OrderBy(rc => rc.BioRegion).ToList();
                    }
                }
                else
                {


                    SqlParameter param1 = new("@idULHeaderSource", idSource);
                    SqlParameter param2 = new("@idULHeaderTarget", idTarget);
                    SqlParameter param3 = new("@bioRegions", string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions);

                    resultCodes = await _dataContext.Set<BioRegionSiteCode>().FromSqlRaw($"exec dbo.spGetBioregionSiteCodesInUnionListComparer  @idULHeaderSource, @idULHeaderTarget, @bioRegions",
                                    param1, param2, param3).ToListAsync();
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(40000);
                    cache.Set(listName, resultCodes, cacheEntryOptions);
                }
                return resultCodes;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - GetBioregionSiteCodesInUnionListComparer", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            try
            {
                UnionListComparerSummaryViewModel res = new();
                if (idSource == null || idTarget == null)
                    return res;
                List<BioRegionSiteCode> resultCodes = await GetBioregionSiteCodesInUnionListComparer(idSource, idTarget, bioRegions, cache);
                res.BioRegSiteCodes = resultCodes.ToList();

                //Get the number of site codes per bio region
                List<BioRegionTypes> ulBioRegions = await GetUnionBioRegionTypes();

                List<UnionListComparerBioReg> codesGrouped = resultCodes.GroupBy(n => n.BioRegion)
                             .Select(n => new UnionListComparerBioReg
                             {
                                 BioRegion = n.Key,
                                 Count = n.Count()
                             }).ToList();
                List<UnionListComparerBioReg> _bioRegionSummary =
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
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - GetCompareSummary", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<UnionListComparerDetailedViewModel>> CompareUnionLists(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache, int page = 1, int pageLimit = 0)
        {
            try
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
                List<UnionListDetail> _ulDetails = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idSource).ToListAsync();
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

                List<UnionListComparerDetailedViewModel> result = new();
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
                    UnionListComparerDetailedViewModel changedItem = new()
                    {
                        BioRegion = item.source1.BioRegion,
                        Sitecode = item.source1.SCI_code,

                        SiteName = new UnionListValues<string>
                        {
                            Source = item.source1.SCI_Name,
                            Target = item.target1.SCI_Name
                        },


                        Priority = new UnionListValues<bool>
                        {
                            Source = item.source1.Priority,
                            Target = item.target1.Priority
                        },


                        Area = new UnionListValues<double>
                        {
                            Source = item.source1.Area,
                            Target = item.target1.Area
                        },

                        Length = new UnionListValues<double>
                        {
                            Source = item.source1.Length,
                            Target = item.target1.Length
                        },

                        Longitude = new UnionListValues<double>
                        {
                            Source = item.source1.Long,
                            Target = item.target1.Long
                        },

                        Latitude = new UnionListValues<double>
                        {
                            Source = item.source1.Lat,
                            Target = item.target1.Lat
                        }
                    };


                    //COMPARE THE VALUES FIELD BY FIELD
                    if ((string?)changedItem.SiteName.Source != (string?)changedItem.SiteName.Target)
                        changedItem.SiteName.Change = "SITENAME Changed";


                    if ((bool?)changedItem.Priority.Source != (bool?)changedItem.Priority.Target)
                    {
                        bool prioSource = ((bool?)changedItem.Priority.Source) ?? false;
                        bool prioTarget = ((bool?)changedItem.Priority.Target) ?? false;

                        if (prioSource && !prioTarget)
                        {
                            changedItem.Priority.Change = "PRIORITY_LOST";
                        }
                        else if (!prioSource && prioTarget)
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
                        double source = ((double?)changedItem.Area.Source) ?? 0.0;
                        double target = ((double?)changedItem.Area.Target) ?? 0.0;

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
                        double source = ((double?)changedItem.Length.Source) ?? 0.0;
                        double target = ((double?)changedItem.Length.Target) ?? 0.0;

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


                //Deleted in target
                var sourceOnlySites = (from source2 in ulDetailsSource
                                       join target2 in ulDetailsTarget on new { source2.SCI_code, source2.BioRegion } equals new { target2.SCI_code, target2.BioRegion } into t
                                       from od in t.DefaultIfEmpty()
                                       where od == null
                                       select source2).ToList();
                foreach (var item in sourceOnlySites)
                {
                    UnionListComparerDetailedViewModel changedItem = new()
                    {
                        BioRegion = item.BioRegion,
                        Sitecode = item.SCI_code,

                        SiteName = new UnionListValues<string>
                        {
                            Source = item.SCI_Name,
                            Target = null
                        },

                        Priority = new UnionListValues<bool>
                        {
                            Source = item.Priority,
                            Target = null
                        },


                        Area = new UnionListValues<double>
                        {
                            Source = item.Area,
                            Target = null
                        },

                        Length = new UnionListValues<double>
                        {
                            Source = item.Length,
                            Target = null
                        },

                        Latitude = new UnionListValues<double>
                        {
                            Source = item.Lat,
                            Target = null
                        },


                        Longitude = new UnionListValues<double>
                        {
                            Source = item.Long,
                            Target = null
                        },

                        Changes = "DELETED"
                    };
                    result.Add(changedItem);
                }


                //Added in target            
                var targetOnlySites = (from target3 in ulDetailsTarget
                                       join source3 in ulDetailsSource on new { target3.SCI_code, target3.BioRegion } equals new { source3.SCI_code, source3.BioRegion } into t
                                       from od in t.DefaultIfEmpty()
                                       where od == null
                                       select target3).ToList();
                foreach (var item in targetOnlySites)
                {
                    UnionListComparerDetailedViewModel changedItem = new()
                    {
                        BioRegion = item.BioRegion,
                        Sitecode = item.SCI_code,

                        SiteName = new UnionListValues<string>
                        {
                            Target = item.SCI_Name,
                            Source = null
                        },

                        Priority = new UnionListValues<bool>
                        {
                            Target = item.Priority,
                            Source = null
                        },


                        Area = new UnionListValues<double>
                        {
                            Target = item.Area,
                            Source = null
                        },

                        Length = new UnionListValues<double>
                        {
                            Target = item.Length,
                            Source = null
                        },

                        Latitude = new UnionListValues<double>
                        {
                            Target = item.Lat,
                            Source = null
                        },


                        Longitude = new UnionListValues<double>
                        {
                            Target = item.Long,
                            Source = null
                        },
                        Changes = "ADDED"
                    };
                    result.Add(changedItem);
                }
                return result.OrderBy(a => a.BioRegion).ThenBy(b => b.Sitecode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - CompareUnionLists", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<string> UnionListDownload(string bioregs)
        {
            IAttachedFileHandler? fileHandler = null;
            string username = GlobalData.Username;
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            if (_appSettings.Value.AttachedFiles.AzureBlob)
            {
                fileHandler = new AzureBlobHandler(_appSettings.Value.AttachedFiles, _dataContext);
            }
            else
            {
                fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles, _dataContext);
            }
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.

            string repositoryPath = string.IsNullOrEmpty(_appSettings.Value.AttachedFiles.FilesRootPath) ?

                Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Value.AttachedFiles.JustificationFolder) :
                Path.Combine(_appSettings.Value.AttachedFiles.FilesRootPath, _appSettings.Value.AttachedFiles.JustificationFolder);

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

            try
            {
                string[] bioRegions;
                if (bioregs.Length > 0)
                {
                    bioRegions = bioregs.Split(',');
                }
                else
                {
                    //List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(a => a.BioRegionShortCode != null).AsNoTracking().ToListAsync();
                    //bioRegions = bioRegionTypes.Select(a => a.BioRegionShortCode).ToArray();
                    bioRegions = "ALP,ATL,MED,BLA,BOR,CON,MAC,PAN,STP".Split(',');
                }

                //Get the current UnionList
                UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

                List<UnionListDetail?> currentUnionListSites = await _dataContext.Set<UnionListDetail>()
                                .AsNoTracking()
                                .Where(ulh => ulh.idUnionListHeader == currentUnionList.idULHeader)
                                .Cast<UnionListDetail?>()
                                .ToListAsync();

                foreach (UnionListDetail currentUnionListSite in currentUnionListSites)
                {
                    List<UnionListDetail?> bioregionCount = currentUnionListSites.Where(l => l.SCI_code == currentUnionListSite.SCI_code && l.version == currentUnionListSite.version).ToList();

                    //If a site only has a marine region it will be reassigned to the corresponding terrestrial bioregion
                    if ((currentUnionListSite.BioRegion == "MAT" || currentUnionListSite.BioRegion == "MBA" || currentUnionListSite.BioRegion == "MBL"
                        || currentUnionListSite.BioRegion == "MMA" || currentUnionListSite.BioRegion == "MME") && bioregionCount.Count == 1)
                    {
                        if (currentUnionListSite.BioRegion == "MBL") //Marine Black Sea
                        {
                            currentUnionListSite.BioRegion = "BLA";
                        }
                        else if (currentUnionListSite.BioRegion == "MMA") //Marine Macaronesian
                        {
                            currentUnionListSite.BioRegion = "MAC";
                        }
                        else if (currentUnionListSite.BioRegion == "MME") //Marine Mediterranean
                        {
                            currentUnionListSite.BioRegion = "MED";
                        }
                        else if (currentUnionListSite.BioRegion == "MAT" && currentUnionListSite.SCI_code[..2] != "DK") //Marine Atlantic
                        {
                            currentUnionListSite.BioRegion = "ATL";
                        }
                        else if (currentUnionListSite.BioRegion == "MBA" && currentUnionListSite.SCI_code[..2] != "DK") //Marine Baltic
                        {
                            if (currentUnionListSite.SCI_code[..2] == "FI" || currentUnionListSite.SCI_code[..2] == "LV"
                                || currentUnionListSite.SCI_code[..2] == "EE" || currentUnionListSite.SCI_code[..2] == "LT")
                            {
                                currentUnionListSite.BioRegion = "BOR";
                            }
                            else if (currentUnionListSite.SCI_code[..2] == "DE" || currentUnionListSite.SCI_code[..2] == "PL")
                            {
                                currentUnionListSite.BioRegion = "CON";
                            }
                        }
                    }
                }

                foreach (string bioRegion in bioRegions)
                {
                    //The file path must be parametrized in the web.config
                    string filePath = repositoryPath + "//" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + bioRegion + "_Union List.xlsx";
                    //Create the Excel document
                    SpreadsheetDocument workbook = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                    //Create the different sections and parts necesaries for the Excel
                    WorkbookPart workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new Workbook
                    {
                        Sheets = new Sheets()
                    };
                    WorksheetPart sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new();
                    sheetPart.Worksheet = new(sheetData);
                    Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);
                    //Just a sheet for the excel book
                    uint sheetId = 1;
                    if (sheets.Elements<Sheet>().Any())
                    {
                        sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    Sheet sheet = new() { Id = relationshipId, SheetId = sheetId, Name = bioRegion }; //Page name = BioRegion name
                    sheets.Append(sheet);

                    try
                    {

                        #region Retrive the data to insert
                        List<UnionListDetailExcel> currentDetails = new();

                        foreach (UnionListDetail site in currentUnionListSites.Where(l => l.BioRegion == bioRegion).ToList())
                        {
                            UnionListDetailExcel temp = new()
                            {
                                SCI_code = site.SCI_code,
                                SCI_Name = site.SCI_Name,
                                Priority = (site.Priority == null) ? "" : ((site.Priority == true) ? "*" : ""),
                                Area = (site.Area == null) ? "" : site.Area.ToString(),
                                Length = (site.Length == null) ? "" : site.Length.ToString(),
                                Long = (site.Long == null) ? "" : site.Long.ToString(),
                                Lat = (site.Lat == null) ? "" : site.Lat.ToString()
                            };
                            currentDetails.Add(temp);
                        }
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

                        Row row = new();
                        Cell cell = new();

                        #region Header of the columns, but we can handwrite it because we know the structure
                        //SCI code
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue("SCI code");
                        cell.StyleIndex = 2;
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Name of SCI
                            DataType = CellValues.String,
                            CellValue = new CellValue("Name of SCI"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Priority
                            DataType = CellValues.String,
                            CellValue = new CellValue("Priority"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Area of SCI (ha)
                            DataType = CellValues.String,
                            CellValue = new CellValue("Area of SCI (ha)"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Length of SCI (km)
                            DataType = CellValues.String,
                            CellValue = new CellValue("Length of SCI (km)"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Longitude
                            DataType = CellValues.String,
                            CellValue = new CellValue("Longitude"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell
                        {
                            //Latitude
                            DataType = CellValues.String,
                            CellValue = new CellValue("Latitude"),
                            StyleIndex = 2
                        };
                        row.AppendChild(cell);
                        cell = new Cell();
                        #endregion

                        sheetData.AppendChild(row);
                        row = new();
                        foreach (UnionListDetailExcel ulde in currentDetails)
                        {
                            #region Content row creation
                            row = new Row();
                            //In the same way we know the structure of the data, so we can call for each field
                            //SCI code
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.SCI_code), //The GetString is because SqlDataReader. With the Entity it's not necesary
                                StyleIndex = 0
                            };
                            row.AppendChild(cell);
                            //Name of SCI
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.SCI_Name),
                                StyleIndex = 0
                            };
                            row.AppendChild(cell);
                            //Priority
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.Priority),
                                StyleIndex = 0
                            };
                            row.AppendChild(cell);
                            //Area of SCI (ha)
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.Area),
                                StyleIndex = 1
                            };
                            row.AppendChild(cell);
                            //Length of SCI (km)
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.Length),
                                StyleIndex = 1
                            };
                            row.AppendChild(cell);
                            //Longitude
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.Long),
                                StyleIndex = 1
                            };
                            row.AppendChild(cell);
                            //Latitude
                            cell = new Cell
                            {
                                DataType = CellValues.String, //It is mandatory and value depends on the type of the data. If not declared, the Excel shows an error in the opening
                                CellValue = new CellValue(ulde.Lat),
                                StyleIndex = 1
                            };
                            row.AppendChild(cell);
                            #endregion

                            sheetData.AppendChild(row);
                        }

                        workbookPart.Workbook.Save();
                        workbook.Close();

                        ZipArchiveEntry fileInZip = archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                        File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        workbook.Close();
                        File.Delete(filePath);
                        throw;
                    }
                }

                archive.Dispose();
                List<String> url = await fileHandler.UploadFileAsync(tempZipFile);

                return url[0];
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - UnionListDownload", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            finally
            {
                archive.Dispose();
            }
        }

        public async Task<UnionListComparerSummaryViewModel> GetUnionListComparerSummary(IMemoryCache _cache)
        {
            try
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
                SqlParameter param1 = new("@name", _appSettings.Value.current_ul_name);
                SqlParameter param2 = new("@creator", _appSettings.Value.current_ul_createdby);
                SqlParameter param3 = new("@final", false);
                await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final ", param1, param2, param3);

                //Get Current
                UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

                return await GetCompareSummary(latestUnionList?.idULHeader, currentUnionList?.idULHeader, null, _cache);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - GetUnionListComparerSummary", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<UnionListComparerDetailedViewModel>> GetUnionListComparer(IMemoryCache _cache, string? bioregions, int page = 1, int pageLimit = 0)
        {
            try
            {
                if (bioregions == null)
                    return new List<UnionListComparerDetailedViewModel>();
                //Get latest release
                UnionListHeader? latestUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name != _appSettings.Value.current_ul_name) && (ulh.CreatedBy != _appSettings.Value.current_ul_createdby) && (ulh.Final == true)).OrderByDescending(ulh => ulh.Date).FirstOrDefaultAsync();

                //Get Current
                UnionListHeader? currentUnionList = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => (ulh.Name == _appSettings.Value.current_ul_name) && (ulh.CreatedBy == _appSettings.Value.current_ul_createdby)).FirstOrDefaultAsync();

                return await CompareUnionLists(latestUnionList.idULHeader, currentUnionList.idULHeader, bioregions, _cache, page, pageLimit);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "UnionListService - GetUnionListComparer", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

    }
}
