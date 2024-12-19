using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.release_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Options;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesContext _releaseContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private const string ulBioRegSites = "ulBioRegSites";

        public ReleaseService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetUnionBioRegionTypes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Releases>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode)
        {
            try
            {
                //List<Releases> releaseHeaders = new List<Releases>();
                List<UnionListHeader> releaseHeaders = new();

                SqlParameter param1 = new("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

                //releaseHeaders = await _releaseContext.Set<Releases>().FromSqlRaw($"exec dbo.spGetReleaseHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();
                releaseHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();

                /*
                if (bioRegionShortCode != null)
                {
                    SqlParameter param1 = new("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

                    //releaseHeaders = await _releaseContext.Set<Releases>().FromSqlRaw($"exec dbo.spGetReleaseHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();
                    releaseHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();
                }
                else
                {
                    //releaseHeaders = await _releaseContext.Set<Releases>().FromSqlRaw($"exec dbo.spGetReleaseHeaders").AsNoTracking().ToListAsync();
                    releaseHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion").AsNoTracking().ToListAsync();
                }
                */

                //releaseHeaders = releaseHeaders.Where(ulh => (ulh.Title != _appSettings.Value.current_ul_name) || (ulh.Author != _appSettings.Value.current_ul_createdby)).ToList();
                releaseHeaders = releaseHeaders.Where(ulh => (ulh.Name != _appSettings.Value.current_ul_name) || (ulh.CreatedBy != _appSettings.Value.current_ul_createdby)).ToList();

                List<Releases> releaseHeadersConverted = await ConvertUnionListHeaderListToReleases(releaseHeaders);

                return releaseHeadersConverted.OrderBy(a => a.CreateDate).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetReleaseHeadersByBioRegion", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Releases>> ConvertUnionListHeaderListToReleases(List<UnionListHeader> unionListHeaders)
        {
            try
            {
                List<Releases> releaseHeaders = new();
                foreach (UnionListHeader unionListHeader in unionListHeaders)
                {
                    Releases releaseHeader = await ConvertUnionListHeaderToReleases(unionListHeader);
                    releaseHeaders.Add(releaseHeader);
                }
                return releaseHeaders;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - ConvertUnionListHeaderListToReleases", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<Releases> ConvertUnionListHeaderToReleases(UnionListHeader unionListHeader)
        {
            try
            {
                await Task.Delay(1);
                Releases releaseHeader = new()
                {
                    ID = unionListHeader.idULHeader,
                    Title = unionListHeader.Name,
                    Author = unionListHeader.CreatedBy,
                    CreateDate = unionListHeader.Date,
                    ModifyDate = unionListHeader.UpdatedDate,
                    Final = unionListHeader.Final,
                    Character = "",
                    ModifyUser = unionListHeader.UpdatedBy
                };
                return releaseHeader;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - ConvertUnionListHeaderToReleases", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<ReleaseDetail>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode)
        {
            try
            {
                SqlParameter param1 = new("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

                List<ReleaseDetail> releaseDetails = await _dataContext.Set<ReleaseDetail>().FromSqlRaw($"exec dbo.spGetCurrentSitesReleaseDetailByBioRegion  @bioregion",
                                param1).AsNoTracking().ToListAsync();

                return releaseDetails;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetCurrentSitesReleaseDetailByBioRegion", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Releases>> GetReleaseHeadersById(long? id)
        {
            try
            {
                List<UnionListHeader> unionListHeader = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).ToListAsync();
                List<Releases> releaseHeader = await ConvertUnionListHeaderListToReleases(unionListHeader);
                return releaseHeader;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetReleaseHeadersById", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<List<BioRegionSiteCode>> GetBioregionSiteCodesInReleaseComparer(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
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
                    SqlParameter param1 = new("@idReleaseSource", idSource);
                    SqlParameter param2 = new("@idReleaseTarget", idTarget);
                    SqlParameter param3 = new("@bioRegions", string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions);

                    resultCodes = await _releaseContext.Set<BioRegionSiteCode>().FromSqlRaw($"exec dbo.spGetBioregionSiteCodesInReleaseComparer  @idReleaseSource, @idReleaseTarget, @bioRegions",
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetBioregionSiteCodesInReleaseComparer", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            try
            {
                UnionListComparerSummaryViewModel res = new();
                List<BioRegionSiteCode> resultCodes = await GetBioregionSiteCodesInReleaseComparer(idSource, idTarget, bioRegions, cache);
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetCompareSummary", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<UnionListComparerDetailedViewModel>> CompareReleases(long? idSource, long? idTarget, string? bioRegions, string? country, IMemoryCache cache, int page = 1, int pageLimit = 0)
        {
            try
            {
                List<BioRegionSiteCode> ulSites = await GetBioregionSiteCodesInReleaseComparer(idSource, idTarget, bioRegions, cache);
                int startRow = (page - 1) * pageLimit;
                if (pageLimit > 0)
                {
                    ulSites = ulSites
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
                }

                //get the bioReg-SiteCodes of the source UL
                SqlParameter param1 = new("@idRelease", idSource);
                List<ReleaseDetail> _ulDetails = await _releaseContext.Set<ReleaseDetail>().FromSqlRaw($"exec dbo.spGetReleaseDetailsById  @idRelease", param1).ToListAsync();
                List<ReleaseDetail> ulDetailsSource = (from src1 in ulSites
                                                       from trgt1 in _ulDetails.Where(trg1 => (src1.SiteCode == trg1.SCI_code) && (src1.BioRegion == trg1.BioRegion))
                                                       select trgt1
                ).ToList();
                _ulDetails.Clear();

                //get the bioReg-SiteCodes of the target UL
                SqlParameter param2 = new("@idRelease", idTarget);
                _ulDetails = await _releaseContext.Set<ReleaseDetail>().FromSqlRaw($"exec dbo.spGetReleaseDetailsById  @idRelease", param2).ToListAsync();
                List<ReleaseDetail> ulDetailsTarget = (from src1 in ulSites
                                                       from trgt2 in _ulDetails.Where(trg2 => (src1.SiteCode == trg2.SCI_code) && (src1.BioRegion == trg2.BioRegion))
                                                       select trgt2
                ).ToList();

                //clear the memory
                _ulDetails.Clear();
                ulSites.Clear();

                if (country != null)
                {
                    ulDetailsSource = ulDetailsSource.Where(uld => uld.SCI_code[..2] == country).ToList();
                    ulDetailsTarget = ulDetailsTarget.Where(uld => uld.SCI_code[..2] == country).ToList();
                }

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
                        bool prioSource = ((bool?)changedItem.Priority.Source).HasValue ? ((bool?)changedItem.Priority.Source).Value : false;
                        bool prioTarget = ((bool?)changedItem.Priority.Target).HasValue ? ((bool?)changedItem.Priority.Target).Value : false;

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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - CompareReleases", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<CountriesAttachmentCountViewModel>> GetCountriesAttachmentCount()
        {
            try
            {
                List<CountriesAttachmentCountViewModel> result = new();
                List<Countries> countries = await _dataContext.Set<Countries>().ToListAsync();
                foreach (Countries c in countries)
                {
                    int documents = _dataContext.Set<JustificationFilesRelease>().AsNoTracking().Where(f => f.CountryCode == c.Code && f.Release == null).Count();
                    int comments = _dataContext.Set<StatusChangesRelease>().AsNoTracking().Where(f => f.CountryCode == c.Code && f.Release == null).Count();
                    result.Add(new CountriesAttachmentCountViewModel
                    {
                        Country = c.Country,
                        Code = c.Code.ToUpper(),
                        NumDocuments = documents,
                        NumComments = comments
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetCountriesAttachmentCount", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        public async Task<ActionResult> DownloadFile(int id, ReleaseProductType filetype)
        {
            try
            {
                //check if the releaseID exists
                UnionListHeader ulh=  await _dataContext.Set<UnionListHeader>().AsNoTracking().FirstOrDefaultAsync(uh => uh.ReleaseID == id);
                if (ulh == null)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, "ReleaseID does not exist", "ReleaseService - Download product file", "", _dataContext.Database.GetConnectionString());
                    return null;
                }
                HttpClient client = new();
                String serverUrl = String.Format(_appSettings.Value.fme_release_product_download, 
                    id.ToString(),
                    _appSettings.Value.Environment, 
                    _appSettings.Value.ReleaseDestDatasetFolder,
                    filetype.ToString(),
                    _appSettings.Value.fme_security_token);
                try
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start Release product generation"), "ReleaseService - Download product file", "", _dataContext.Database.GetConnectionString());
                    client.Timeout = TimeSpan.FromHours(5);
                    Task<HttpResponseMessage> response = client.GetAsync(serverUrl, HttpCompletionOption.ResponseHeadersRead);
                    Stream content = await response.Result.Content.ReadAsStreamAsync(); 
                    string filename = response.Result.Content.Headers.ContentDisposition.FileNameStar;

                    return new FileContentResult(TypeConverters.StreamToByteArray(content), "application/octet-stream")
                    {
                        FileDownloadName = filename
                    };
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - Download product file", "", _dataContext.Database.GetConnectionString());
                    return null;
                }
                finally
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End Release product generation"), "ReleaseService - Download product file", "", _dataContext.Database.GetConnectionString());
                    client.Dispose();
                }
                

            }

            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - Download product file", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        #region CountryDocuments
        public async Task<List<JustificationFilesRelease>> GetCountryDocuments(string country)
        {
            try
            {
                List<JustificationFilesRelease> result = _dataContext.Set<JustificationFilesRelease>().AsNoTracking().Where(f => f.CountryCode == country && f.Release == null).ToList();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetCountryDocuments", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<JustificationFilesRelease>> AddCountryDocument(AttachedFileRelease attachedFile)
        {
            try
            {
                List<JustificationFilesRelease> result = new();
                IAttachedFileHandler? fileHandler = null;
                string username = GlobalData.Username;

                if (_appSettings.Value.AttachedFiles == null) return result;

                if (_appSettings.Value.AttachedFiles.AzureBlob)
                {
                    fileHandler = new AzureBlobHandler(_appSettings.Value.AttachedFiles, _dataContext);
                }
                else
                {
                    fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles, _dataContext);
                }
                List<JustificationFiles> fileUrl = await fileHandler.UploadFileAsync(new AttachedFile() { Files = attachedFile.Files });
                foreach (JustificationFiles fUrl in fileUrl)
                {
                    JustificationFilesRelease justFile = new()
                    {
                        Path = fUrl.Path,
                        OriginalName = fUrl.OriginalName,
                        CountryCode = attachedFile.Country,
                        ImportDate = DateTime.Now,
                        Username = username,
                        Comment = attachedFile.Comment,
                    };
                    await _dataContext.Set<JustificationFilesRelease>().AddAsync(justFile);
                    await _dataContext.SaveChangesAsync();
                }
                return await GetCountryDocuments(attachedFile.Country);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - AddCountryDocument", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<JustificationFilesRelease>> DeleteCountryDocument(long fileId)
        {
            try
            {
                JustificationFilesRelease file = _dataContext.Set<JustificationFilesRelease>().First(f => f.ID == fileId);
                if (file != null)
                {
                    _dataContext.Set<JustificationFilesRelease>().Remove(file);
                    await _dataContext.SaveChangesAsync();
                    return await GetCountryDocuments(file.CountryCode);
                }
                return new List<JustificationFilesRelease>();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - DeleteCountryDocument", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
        #endregion

        #region CountryComments
        public async Task<List<StatusChangesRelease>> GetCountryComments(string country)
        {
            try
            {
                List<StatusChangesRelease> result = _dataContext.Set<StatusChangesRelease>().AsNoTracking().Where(f => f.CountryCode == country && f.Release == null).ToList();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - GetCountryComments", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<StatusChangesRelease>> AddCountryComment(StatusChangesRelease comment)
        {
            try
            {
                List<StatusChangesRelease> result = new();
                comment.Date = DateTime.Now;
                comment.Owner = GlobalData.Username;
                comment.Edited = 0;
                comment.EditedBy = null;
                comment.EditedDate = null;
                await _dataContext.Set<StatusChangesRelease>().AddAsync(comment);
                await _dataContext.SaveChangesAsync();
                result = await _dataContext.Set<StatusChangesRelease>().AsNoTracking().Where(ch => ch.CountryCode == comment.CountryCode).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - AddCountryComment", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<StatusChangesRelease>> UpdateCountryComment(StatusChangesRelease comment)
        {
            try
            {
                StatusChangesRelease prev = _dataContext.Set<StatusChangesRelease>().Where(c => c.Id == comment.Id).OrderBy(c => c.Edited).Last();
                if (prev != null)
                {
                    prev.Edited++;
                    prev.EditedDate = DateTime.Now;
                    prev.EditedBy = GlobalData.Username;
                    prev.Comments = comment.Comments;
                    _dataContext.Set<StatusChangesRelease>().Update(prev);
                    await _dataContext.SaveChangesAsync();
                }
                return await GetCountryComments(comment.CountryCode);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - UpdateCountryComment", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<StatusChangesRelease>> DeleteCountryComment(long commentId)
        {
            try
            {
                List<StatusChangesRelease> comments = _dataContext.Set<StatusChangesRelease>().Where(c => c.Id == commentId).ToList();
                if (comments.Any())
                {
                    foreach (StatusChangesRelease c in comments)
                    {
                        _dataContext.Set<StatusChangesRelease>().Remove(c);
                    }
                    await _dataContext.SaveChangesAsync();
                    return await GetCountryComments(comments.First().CountryCode);
                }

                return new List<StatusChangesRelease>();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - DeleteCountryComment", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
        #endregion

        public async Task<List<Releases>> CreateRelease(string title, Boolean? Final, string? character)
        {
            try
            {
                SqlParameter param1 = new("@Title", title);
                SqlParameter param2 = new("@Author", GlobalData.Username);
                SqlParameter param3 = new("@CreateDate", DateTime.Now);
                SqlParameter param4 = new("@ModifyDate", DateTime.Now);
                SqlParameter param5 = new("@Final", Final);
                SqlParameter param6 = new("@Character", string.IsNullOrEmpty(character) ? string.Empty : character);
                List<Releases> releaseID = await _releaseContext.Set<Releases>().FromSqlRaw("exec dbo.createNewRelease  @Title, @Author, @CreateDate, @ModifyDate, @Final, @Character", param1, param2, param3, param4, param5, param6).AsNoTracking().ToListAsync();

                //call the FME service that creates the SHP, MDB and GPKG

                if (releaseID.Count > 0)
                {
                    long _releaseID = releaseID.ElementAt(0).ID;

                    //call the FME in Async mode and do not wait for it.
                    //FME will send an email to the user when it´s finished
                    HttpClient client = new();
                    try
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Launch FME release creation", "CreateRelease", "", _dataContext.Database.GetConnectionString());
                        client.Timeout = TimeSpan.FromHours(5);
                        string url = string.Format("{0}/fmerest/v3/transformations/submit/{1}/{2}",
                           _appSettings.Value.fme_service_release.server_url,
                           _appSettings.Value.fme_service_release.repository,
                           _appSettings.Value.fme_service_release.workspace);

                        string body = string.Format(@"{{""publishedParameters"":[" +
                            @"{{""name"":""ReleaseId"",""value"":{0}}}," +
                            @"{{""name"":""DestDatasetFolder"",""value"":""{1}""}}," +
                            @"{{""name"":""OutputName"",""value"": ""{2}""}}," +
                            @"{{""name"":""Environment"",""value"": ""{3}""}}," +
                            @"{{""name"":""EMail"",""value"": ""{4}""}}]" +
                            @"}}", _releaseID, _appSettings.Value.ReleaseDestDatasetFolder, title, _appSettings.Value.Environment, GlobalData.Username);

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + _appSettings.Value.fme_security_token);
                        client.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                        HttpRequestMessage request = new(HttpMethod.Post, url)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")//CONTENT-TYPE header
                        };

                        //call the FME script in async 
                        var res = await client.SendAsync(request);
                        //get the JobId 
                        var json = await res.Content.ReadAsStringAsync();
                        JObject jResponse = JObject.Parse(json);
                        string jobId = jResponse.GetValue("id").ToString();
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("FME release creation Launched with jobId:{0}", jobId), "CreateRelease", "", _dataContext.Database.GetConnectionString());
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("Error Launching FME:{0}", ex.Message), "CreateRelease", "", _dataContext.Database.GetConnectionString());
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }

                //Create UnionList entry
                SqlParameter param8 = new("@name", title);
                SqlParameter param9 = new("@creator", GlobalData.Username);
                SqlParameter param10 = new("@final", Final);
                SqlParameter param11 = new("@release", releaseID.First().ID);
                await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewReleaseUnionList  @name, @creator, @final, @release ", param8, param9, param10, param11);

                return await GetReleaseHeadersByBioRegion(null);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - CreateRelease", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Releases>> UpdateRelease(long id, string name, Boolean final)
        {
            try
            {
                UnionListHeader unionlistheader = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).FirstOrDefaultAsync();
                if (unionlistheader != null)
                {
                    if (name != "string")
                        unionlistheader.Name = name;

                    unionlistheader.Final = final;
                    unionlistheader.UpdatedBy = GlobalData.Username;
                    unionlistheader.UpdatedDate = DateTime.Now;

                    _dataContext.Set<UnionListHeader>().Update(unionlistheader);
                }
                await _dataContext.SaveChangesAsync();

                Releases release = await _releaseContext.Set<Releases>().AsNoTracking().Where(ulh => ulh.ID == unionlistheader.ReleaseID).FirstOrDefaultAsync();
                if (release != null)
                {
                    if (name != "string")
                        release.Title = name;

                    release.Final = final;
                    release.ModifyUser = GlobalData.Username;
                    release.ModifyDate = DateTime.Now;

                    _releaseContext.Set<Releases>().Update(release);
                }
                await _releaseContext.SaveChangesAsync();

                return await GetReleaseHeadersByBioRegion(null);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - UpdateRelease", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> DeleteRelease(long id)
        {
            try
            {
                int result = 0;

                List<Lineage> list = await _dataContext.Set<Lineage>().Where(l => l.Release == id).ToListAsync();
                list.Select(c => { c.Release = null; return c; }).ToList();

                UnionListHeader? unionlistheader = await _dataContext.Set<UnionListHeader>().AsNoTracking().FirstOrDefaultAsync(ulh => ulh.idULHeader == id);
                if (unionlistheader != null)
                {
                    _dataContext.Set<UnionListHeader>().Remove(unionlistheader);
                    await _dataContext.SaveChangesAsync();

                    //Delete assignment to Release from Attachments and comments
                    SqlParameter param1 = new("@id", unionlistheader.idULHeader);
                    await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spDeleteReleaseUnionList  @id", param1);

                    result = 1;
                }

                Releases? release = await _releaseContext.Set<Releases>().AsNoTracking().FirstOrDefaultAsync(ulh => ulh.ID == unionlistheader.ReleaseID);
                if (release != null)
                {
                    _releaseContext.Set<Releases>().Remove(release);
                    await _releaseContext.SaveChangesAsync();
                    result = 1;
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseService - DeleteRelease", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

    }
}
