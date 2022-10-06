using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;

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

        public async Task<List<UnionListComparerViewModel>> CompareUnionLists(long? idSource, long? idTarget)
        {
            List<UnionListDetail> ULDetailsSource = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idSource).ToListAsync();
            List<UnionListDetail> ULDetailsTarget = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idTarget).ToListAsync();

            List<UnionListComparerViewModel> result = new List<UnionListComparerViewModel>();

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
                UnionListComparerViewModel changedItem = new UnionListComparerViewModel();
                changedItem.BioRegion = item.source1.BioRegion;
                changedItem.Sitecode = item.source1.SCI_code;

                changedItem.SitenameSourceValue = item.source1.SCI_Name;
                changedItem.SitenameTargetValue = item.target1.SCI_Name;
                changedItem.PrioritySourceValue = item.source1.Priority;
                changedItem.PriorityTargetValue = item.target1.Priority;
                changedItem.AreaSourceValue = item.source1.Area;
                changedItem.AreaTargetValue = item.target1.Area;
                changedItem.LengthSourceValue = item.source1.Length;
                changedItem.LengthTargetValue = item.target1.Length;
                changedItem.LatitudeSourceValue = item.source1.Lat;
                changedItem.LatitudeTargetValue = item.target1.Lat;
                changedItem.LongitudeSourceValue = item.source1.Long;
                changedItem.LongitudeTargetValue = item.target1.Long;

                //COMPARE THE VALUES FIELD BY FIELD
                if (changedItem.SitenameSourceValue != changedItem.SitenameTargetValue)
                    changedItem.Changes.Add("SITENAME");
                if (changedItem.PrioritySourceValue != changedItem.PriorityTargetValue)
                {
                    if (changedItem.PrioritySourceValue == true && changedItem.PriorityTargetValue == false)
                    {
                        changedItem.Changes.Add("PRIORITY_LOST");
                    }
                    else if (changedItem.PrioritySourceValue == false && changedItem.PriorityTargetValue == true)
                    {
                        changedItem.Changes.Add("PRIORITY_GAIN");
                    }
                    else
                    {
                        changedItem.Changes.Add("PRIORITY_CHANGED");
                    }
                }
                if (changedItem.AreaSourceValue != changedItem.AreaTargetValue)
                {
                    if (changedItem.AreaSourceValue < changedItem.AreaTargetValue)
                    {
                        changedItem.Changes.Add("AREA_INCREASED");
                    }
                    else if (changedItem.AreaSourceValue > changedItem.AreaTargetValue)
                    {
                        changedItem.Changes.Add("AREA_DECREASED");
                    }
                    else
                    {
                        changedItem.Changes.Add("AREA_CHANGED");
                    }
                }
                if (changedItem.LengthSourceValue != changedItem.LengthTargetValue)
                {
                    if (changedItem.LengthSourceValue < changedItem.LengthTargetValue)
                    {
                        changedItem.Changes.Add("LENGTH_INCREASED");
                    }
                    else if (changedItem.LengthSourceValue > changedItem.LengthTargetValue)
                    {
                        changedItem.Changes.Add("LENGTH_DECREASED");
                    }
                    else
                    {
                        changedItem.Changes.Add("LENGTH_CHANGED");
                    }
                }
                if (changedItem.LatitudeSourceValue != changedItem.LatitudeTargetValue)
                {
                    if (changedItem.LatitudeSourceValue < changedItem.LatitudeTargetValue)
                    {
                        changedItem.Changes.Add("LATITUDE_INCREASED");
                    }
                    else if (changedItem.LatitudeSourceValue > changedItem.LatitudeTargetValue)
                    {
                        changedItem.Changes.Add("LATITUDE_DECREASED");
                    }
                    else
                    {
                        changedItem.Changes.Add("LATITUDE_CHANGED");
                    }
                }
                if (changedItem.LongitudeSourceValue != changedItem.LongitudeTargetValue)
                {
                    if (changedItem.LongitudeSourceValue < changedItem.LongitudeTargetValue)
                    {
                        changedItem.Changes.Add("LONGITUDE_INCREASED");
                    }
                    else if (changedItem.LongitudeSourceValue > changedItem.LongitudeTargetValue)
                    {
                        changedItem.Changes.Add("LONGITUDE_DECREASED");
                    }
                    else
                    {
                        changedItem.Changes.Add("LONGITUDE_CHANGED");
                    }
                }

                result.Add(changedItem);
            }

            //Added in source
            var sourceOnlySites = ULDetailsTarget.Where(trg => !ULDetailsSource.Any(src => (trg.SCI_code == src.SCI_code) && (trg.BioRegion == src.BioRegion)));

            foreach (var item in sourceOnlySites)
            {
                UnionListComparerViewModel changedItem = new UnionListComparerViewModel();
                changedItem.BioRegion = item.BioRegion;
                changedItem.Sitecode = item.SCI_code;

                changedItem.SitenameSourceValue = item.SCI_Name;
                changedItem.SitenameTargetValue = null;
                changedItem.PrioritySourceValue = item.Priority;
                changedItem.PriorityTargetValue = null;
                changedItem.AreaSourceValue = item.Area;
                changedItem.AreaTargetValue = null;
                changedItem.LengthSourceValue = item.Length;
                changedItem.LengthTargetValue = null;
                changedItem.LatitudeSourceValue = item.Lat;
                changedItem.LatitudeTargetValue = null;
                changedItem.LongitudeSourceValue = item.Long;
                changedItem.LongitudeTargetValue = null;

                changedItem.Changes.Add("ADDED");

                result.Add(changedItem);
            }

            //Deleted in source
            var targetOnlySites = ULDetailsSource.Where(src => !ULDetailsTarget.Any(trg => (src.SCI_code == trg.SCI_code) && (src.BioRegion == trg.BioRegion)));

            foreach (var item in targetOnlySites)
            {
                UnionListComparerViewModel changedItem = new UnionListComparerViewModel();
                changedItem.BioRegion = item.BioRegion;
                changedItem.Sitecode = item.SCI_code;

                changedItem.SitenameSourceValue = null;
                changedItem.SitenameTargetValue = item.SCI_Name;
                changedItem.PrioritySourceValue = null;
                changedItem.PriorityTargetValue = item.Priority;
                changedItem.AreaSourceValue = null;
                changedItem.AreaTargetValue = item.Area;
                changedItem.LengthSourceValue = null;
                changedItem.LengthTargetValue = item.Length;
                changedItem.LatitudeSourceValue = null;
                changedItem.LatitudeTargetValue = item.Lat;
                changedItem.LongitudeSourceValue = null;
                changedItem.LongitudeTargetValue = item.Long;

                changedItem.Changes.Add("DELETED");

                result.Add(changedItem);
            }

            return result.OrderBy(a => a.BioRegion).ThenBy(b => b.Sitecode).ToList();
        }

        public async Task<UnionListHeader> CreateUnionList(string name, Boolean final)
        {
            UnionListHeader unionList = new UnionListHeader();
            unionList.Name = name;
            unionList.Date = DateTime.Now;
            unionList.CreatedBy = GlobalData.Username;
            unionList.Final = final;

            _dataContext.Set<UnionListHeader>().Add(unionList);
            await _dataContext.SaveChangesAsync();

            SqlParameter param1 = new SqlParameter("@bioregion", string.Empty);
            List<UnionListDetail> unionListDetails = await _dataContext.Set<UnionListDetail>().FromSqlRaw($"exec dbo.spGetCurrentSitesUnionListDetailByBioRegion  @bioregion",
                            param1).AsNoTracking().ToListAsync();
            unionListDetails.ForEach(c => { c.idUnionListHeader = unionList.idULHeader; });

            _dataContext.Set<UnionListDetail>().AddRange(unionListDetails);
            await _dataContext.SaveChangesAsync();

            return unionList;
        }

        public async Task<UnionListHeader> EditUnionList(long id, string name, Boolean final)
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

            return await _dataContext.Set<UnionListHeader>().AsNoTracking().Where(ulh => ulh.idULHeader == id).FirstOrDefaultAsync();
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
