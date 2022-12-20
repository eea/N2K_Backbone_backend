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
using System.Data;
using DocumentFormat.OpenXml.ExtendedProperties;

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
            return await _dataContext.Set<BioRegionTypes>().AsNoTracking().Where(bio => bio.BioRegionShortCode != null).ToListAsync();
        }

        public async Task<List<UnionListHeader>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode)
        {
            //List<Releases> releaseHeaders = new List<Releases>();
            List<UnionListHeader> releaseHeaders = new List<UnionListHeader>();

            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            //releaseHeaders = await _releaseContext.Set<Releases>().FromSqlRaw($"exec dbo.spGetReleaseHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();
            releaseHeaders = await _dataContext.Set<UnionListHeader>().FromSqlRaw($"exec dbo.spGetUnionListHeadersByBioRegion  @bioregion", param1).AsNoTracking().ToListAsync();

            /*
            if (bioRegionShortCode != null)
            {
                SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

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
            releaseHeaders = releaseHeaders.Where(ulh => (ulh.Title != _appSettings.Value.current_ul_name) || (ulh.CreatedBy != _appSettings.Value.current_ul_createdby)).ToList();
            return releaseHeaders;
        }

        public async Task<List<ReleaseDetail>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode)
        {
            SqlParameter param1 = new SqlParameter("@bioregion", string.IsNullOrEmpty(bioRegionShortCode) ? string.Empty : bioRegionShortCode);

            List<ReleaseDetail> releaseDetails = await _dataContext.Set<ReleaseDetail>().FromSqlRaw($"exec dbo.spGetCurrentSitesReleaseDetailByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();

            return releaseDetails;
        }

        public async Task<List<Releases>> GetReleaseHeadersById(long? id)
        {
            return await _releaseContext.Set<Releases>().AsNoTracking().Where(ulh => ulh.ID == id).ToListAsync();
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

                SqlParameter param1 = new SqlParameter("@idReleaseSource", idSource);
                SqlParameter param2 = new SqlParameter("@idReleaseTarget", idTarget);
                SqlParameter param3 = new SqlParameter("@bioRegions", string.IsNullOrEmpty(bioRegions) ? string.Empty : bioRegions);

                resultCodes = await _releaseContext.Set<BioRegionSiteCode>().FromSqlRaw($"exec dbo.spGetBioregionSiteCodesInReleaseComparer  @idReleaseSource, @idReleaseTarget, @bioRegions",
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


        public async Task<List<UnionListComparerDetailedViewModel>> CompareReleases(long? idSource, long? idTarget, string? bioRegions, string? country, IMemoryCache cache, int page = 1, int pageLimit = 0)
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
            SqlParameter param1 = new SqlParameter("@idRelease", idSource);
            var _ulDetails = await _releaseContext.Set<ReleaseDetail>().FromSqlRaw($"exec dbo.spGetReleaseDetailsById  @idRelease", param1).ToListAsync();
            List<ReleaseDetail> ulDetailsSource = (from src1 in ulSites
                                                   from trgt1 in _ulDetails.Where(trg1 => (src1.SiteCode == trg1.SCI_code) && (src1.BioRegion == trg1.BioRegion))
                                                   select trgt1
            ).ToList();
            _ulDetails.Clear();

            //get the bioReg-SiteCodes of the target UL
            SqlParameter param2 = new SqlParameter("@idRelease", idTarget);
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
                ulDetailsSource = ulDetailsSource.Where(uld => (uld.SCI_code.Substring(0, 2) == country)).ToList();
                ulDetailsTarget = ulDetailsTarget.Where(uld => (uld.SCI_code.Substring(0, 2) == country)).ToList();
            }

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

                changedItem.Changes = "DELETED";
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
                changedItem.Changes = "ADDED";
                result.Add(changedItem);
            }
            return result.OrderBy(a => a.BioRegion).ThenBy(b => b.Sitecode).ToList();
        }

        public async Task<List<UnionListHeader>> CreateRelease(string title, Boolean? isOfficial, string? character, string? comments)
        {
            SqlParameter param1 = new SqlParameter("@Title", title);
            SqlParameter param2 = new SqlParameter("@Author", GlobalData.Username);
            SqlParameter param3 = new SqlParameter("@CreateDate", DateTime.Now);
            SqlParameter param4 = new SqlParameter("@ModifyDate", DateTime.Now);
            SqlParameter param5 = new SqlParameter("@IsOfficial", isOfficial);

            SqlParameter param6 = new SqlParameter("@Character", string.IsNullOrEmpty(character) ? string.Empty : character);
            SqlParameter param7 = new SqlParameter("@Comments", string.IsNullOrEmpty(comments) ? string.Empty : comments);

            List<Releases> releaseID = await _releaseContext.Set<Releases>().FromSqlRaw("exec dbo.createNewRelease  @Title, @Author, @CreateDate, @ModifyDate, @IsOfficial, @Character, @Comments", param1, param2, param3, param4, param5, param6, param7).AsNoTracking().ToListAsync();

            //Create UnionList entry
            SqlParameter param8 = new SqlParameter("@name", title);
            SqlParameter param9 = new SqlParameter("@creator", GlobalData.Username);
            SqlParameter param10 = new SqlParameter("@final", isOfficial);
            SqlParameter param11 = new SqlParameter("@release", releaseID.First().ID);
            await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spCreateNewUnionList  @name, @creator, @final, @release ", param8, param9, param10, param11);

            return await GetReleaseHeadersByBioRegion(null);
        }

        public async Task<List<UnionListHeader>> UpdateRelease(long id, string name, Boolean final)
        {
            Releases release = await _releaseContext.Set<Releases>().AsNoTracking().Where(ulh => ulh.ID == id).FirstOrDefaultAsync();
            if (release != null)
            {
                if (name != "string")
                    release.Title = name;

                release.IsOfficial = final;
                release.ModifyUser = GlobalData.Username;
                release.ModifyDate = DateTime.Now;

                _releaseContext.Set<Releases>().Update(release);
            }
            await _releaseContext.SaveChangesAsync();

            UnionListHeader unionlistheader = await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.ReleaseID == id).FirstOrDefaultAsync();
            if (unionlistheader != null)
            {
                if (name != "string")
                    unionlistheader.Title = name;

                unionlistheader.Final = final;
                unionlistheader.UpdatedBy = GlobalData.Username;
                unionlistheader.UpdatedDate = DateTime.Now;

                _dataContext.Set<UnionListHeader>().Update(unionlistheader);
            }
            await _dataContext.SaveChangesAsync();

            return await GetReleaseHeadersByBioRegion(null);
        }

        public async Task<int> DeleteRelease(long id)
        {
            int result = 0;
            Releases? release = await _releaseContext.Set<Releases>().AsNoTracking().FirstOrDefaultAsync(ulh => ulh.ID == id);
            if (release != null)
            {
                _releaseContext.Set<Releases>().Remove(release);
                await _releaseContext.SaveChangesAsync();
                result = 1;
            }

            UnionListHeader? unionlistheader = await _dataContext.Set<UnionListHeader>().AsNoTracking().FirstOrDefaultAsync(ulh => ulh.ReleaseID == id);
            if (unionlistheader != null)
            {
                _dataContext.Set<UnionListHeader>().Remove(unionlistheader);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;
        }

    }
}
