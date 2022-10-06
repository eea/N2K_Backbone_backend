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

        public async Task<List<UnionListDetail>> CompareUnionLists(long? idTarget, long? idSource)
        {
            return await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(uld => uld.idUnionListHeader == idTarget).ToListAsync();
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
