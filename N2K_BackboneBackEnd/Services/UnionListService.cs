using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.Diagnostics.Metrics;
using System.Linq;

namespace N2K_BackboneBackEnd.Services
{
    public class UnionListService : IUnionListService
    {
        private readonly N2KBackboneContext _dataContext;

        public UnionListService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
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


        public async Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget)
        {
            SqlParameter param1 = new SqlParameter("@idULHeaderSource", idSource);
            SqlParameter param2 = new SqlParameter("@idULHeaderTarget", idTarget);
            List<BioRegionSiteCode> resultCodes = await _dataContext.Set<BioRegionSiteCode>().FromSqlRaw($"exec dbo.spGetBioregionSiteCodesInUnionListComparer  @idULHeaderSource, @idULHeaderTarget",
                            param1, param2).ToListAsync();
           
            UnionListComparerSummaryViewModel res = new UnionListComparerSummaryViewModel();
            res.BioRegSiteCodes = resultCodes;

            //Get the number of site codes per bio region
            List<BioRegionTypes> bioRegions = await GetUnionBioRegionTypes();

            var codesGrouped = resultCodes.GroupBy(n => n.BioRegion)
                         .Select(n => new UnionListComparerBioReg
                         {
                             BioRegion = n.Key,
                             Count = n.Count()
                         }).ToList();
            var _bioRegionSummary =
                (
                from p in bioRegions
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


        public async Task<List<UnionListComparerDetailedViewModel>> CompareUnionLists(long? idSource, long? idTarget,int? page,int? limit)
        {
            List<UnionListDetail> ULDetailsSource = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idSource).ToListAsync();
            List<UnionListDetail> ULDetailsTarget = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idTarget).ToListAsync();

            List<UnionListComparerDetailedViewModel> result = new List<UnionListComparerDetailedViewModel>();

            //Changed
            var changedSites = (from source1 in ULDetailsSource
                                join target1 in ULDetailsTarget
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

            /*            
            //Added in source
            var sourceOnlySites = (from source2 in ULDetailsSource
                                   from target2 in ULDetailsTarget.Where(trg => (source2.SCI_code == trg.SCI_code) && (source2.BioRegion == trg.BioRegion))
                                   where target2.SCI_code == null
                                   select new { source2, target2 }).ToList();

            foreach (var item in sourceOnlySites)
            {
                UnionListComparerDetailedViewModel changedItem = new UnionListComparerDetailedViewModel();
                changedItem.BioRegion = item.source2.BioRegion;
                changedItem.Sitecode = item.source2.SCI_code;

                changedItem.SiteName = new UnionListValues<string>
                {
                    Source = item.source2.SCI_Name,
                    Target = null
                };


                changedItem.Area  = new UnionListValues<double>
                {
                    Source = item.source2.Area,
                    Target = null
                };

                changedItem.Length = new UnionListValues<double>
                {
                    Source = item.source2.Length,
                    Target = null
                };

                changedItem.Latitude = new UnionListValues<double>
                {
                    Source = item.source2.Lat,
                    Target = null
                };


                changedItem.Longitude = new UnionListValues<double>
                {
                    Source = item.source2.Long,
                    Target = null
                };

                changedItem.Changes= "ADDED";
                result.Add(changedItem);
            }

            
            //Deleted in source
            var targetOnlySites = (from target3 in ULDetailsTarget
                                   from source3 in ULDetailsSource.Where(trg => (target3.SCI_code == trg.SCI_code) && (target3.BioRegion == trg.BioRegion))
                                   where source3.SCI_code == null
                                   select new { source3, target3 }).ToList();

            foreach (var item in targetOnlySites)
            {
                UnionListComparerDetailedViewModel changedItem = new UnionListComparerDetailedViewModel();
                changedItem.BioRegion = item.target3.BioRegion;
                changedItem.Sitecode = item.target3.SCI_code;

                changedItem.SiteName = new UnionListValues<string>
                {
                    Target = item.target3.SCI_Name,
                    Source = null
                };


                changedItem.Area = new UnionListValues<double>
                {
                    Target = item.target3.Area,
                    Source = null
                };

                changedItem.Length = new UnionListValues<double>
                {
                    Target = item.target3.Length,
                    Source = null
                };

                changedItem.Latitude = new UnionListValues<double>
                {
                    Target = item.target3.Lat,
                    Source = null
                };


                changedItem.Longitude = new UnionListValues<double>
                {
                    Target = item.target3.Long,
                    Source = null
                };
                changedItem.Changes= "DELETED";
                result.Add(changedItem);
            }
            */
            return result.OrderBy(a => a.BioRegion).ThenBy(b => b.Sitecode).Take(10).ToList();
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
    }
}
