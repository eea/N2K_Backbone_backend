using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface IReleaseService
    {
        Task<List<BioRegionTypes>> GetUnionBioRegionTypes();
        Task<List<UnionListHeader>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListDetail>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListHeader>> GetReleaseHeadersById(long? id);
        Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache);
        Task<List<UnionListComparerDetailedViewModel>> CompareReleases(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache ,int page = 1, int pageLimit = 0);
        Task<List<UnionListHeader>> CreateRelease(string name, Boolean final);
        Task<List<UnionListHeader>> UpdateRelease(long id, string name, Boolean final);
        Task<int> DeleteRelease(long id);
    }
}
