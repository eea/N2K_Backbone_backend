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
    public class ReleaseService : IReleaseService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private const string ulBioRegSites = "ulBioRegSites";

        public ReleaseService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

        public async Task<List<BioRegionTypes>> GetUnionBioRegionTypes()
        {
            return await _dataContext.Set<BioRegionTypes>().AsNoTracking().Where(bio => bio.BioRegionShortCode != null).ToListAsync();
        }

        public async Task<List<UnionListHeader>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode)
        {
            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            List<UnionListHeader> unionListHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();
            unionListHeaders = unionListHeaders.Where(ulh => (ulh.Name != _appSettings.Value.current_ul_name) || (ulh.CreatedBy != _appSettings.Value.current_ul_createdby)).ToList();
            return unionListHeaders;
        }

        public async Task<List<UnionListDetail>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode)
        {
            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            List<UnionListDetail> unionListDetails = await _dataContext.Set<UnionListDetail>().FromSqlRaw($"exec dbo.spGetCurrentSitesUnionListDetailByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();

            return unionListDetails;
        }

        public async Task<List<UnionListHeader>> GetReleaseHeadersById(long? id)
        {
            return await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).ToListAsync();
        }

        private async Task<List<BioRegionSiteCode>> GetBioregionSiteCodesInReleaseComparer(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache)
        {
            string listName = string.Format("{0}_{1}_{2}_{3}", GlobalData.Username, ulBioRegSites, idSource, idTarget);
            List<BioRegionSiteCode> resultCodes = new List<BioRegionSiteCode>();
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
                if (!string.IsNullOrEmpty(bioRegions))
                {
                    listName = string.Format("{0}_{1}_{2}_{3}_{4}", GlobalData.Username, ulBioRegSites, idSource, idTarget, string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions.Replace(",", "_"));
                }

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
            List<BioRegionSiteCode> resultCodes = await GetBioregionSiteCodesInReleaseComparer(idSource, idTarget, bioRegions, cache);
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


        public async Task<List<UnionListComparerDetailedViewModel>> CompareReleases(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache, int page = 1, int pageLimit = 0)
        {
            List<BioRegionSiteCode> ulSites = await GetBioregionSiteCodesInReleaseComparer(idSource, idTarget, bioRegions, cache);
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

        public async Task<List<UnionListHeader>> CreateRelease(string name, Boolean final)
        {
            SqlParameter param1 = new SqlParameter("@name", name);
            SqlParameter param2 = new SqlParameter("@creator", GlobalData.Username);
            SqlParameter param3 = new SqlParameter("@final", final);

            await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final ", param1, param2, param3);
            return await GetReleaseHeadersByBioRegion(null);
        }

        public async Task<List<UnionListHeader>> UpdateRelease(long id, string name, Boolean final)
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

            return await GetReleaseHeadersByBioRegion(null);


        }

        public async Task<int> DeleteRelease(long id)
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

    }
}
