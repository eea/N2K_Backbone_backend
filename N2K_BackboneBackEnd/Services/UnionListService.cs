using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
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
    }
}
