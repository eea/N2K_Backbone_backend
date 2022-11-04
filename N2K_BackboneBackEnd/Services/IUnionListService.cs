using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface IUnionListService
    {
        Task<List<BioRegionTypes>> GetUnionBioRegionTypes();
        Task<List<UnionListHeader>> GetUnionListHeadersByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListDetail>> GetCurrentSitesUnionListDetailByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListHeader>> GetUnionListHeadersById(long? id);
        Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache);
        Task<List<UnionListComparerDetailedViewModel>> CompareUnionLists(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache ,int page = 1, int pageLimit = 0);
        Task<List<UnionListHeader>> CreateUnionList(string name, Boolean final);
        Task<List<UnionListHeader>> UpdateUnionList(long id, string name, Boolean final);
        Task<int> DeleteUnionList(long id);
        Task<string> UnionListDownload(string bioregs);
        Task<UnionListComparerSummaryViewModel> GetUnionListComparerSummary(IMemoryCache _cache);
        Task<List<UnionListComparerDetailedViewModel>> GetUnionListComparer(IMemoryCache _cache, string? bioregions, int page = 1, int pageLimit = 0);
    }
}
