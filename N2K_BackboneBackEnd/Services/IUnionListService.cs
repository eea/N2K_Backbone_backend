using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface IUnionListService
    {
        Task<List<BioRegionTypes>> GetUnionBioRegionTypes();
        Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache);
        Task<List<UnionListComparerDetailedViewModel>> CompareUnionLists(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache ,int page = 1, int pageLimit = 0);
        Task<string> UnionListDownload(string bioregs);
        Task<UnionListComparerSummaryViewModel> GetUnionListComparerSummary(IMemoryCache _cache);
        Task<List<UnionListComparerDetailedViewModel>> GetUnionListComparer(IMemoryCache _cache, string? bioregions, int page = 1, int pageLimit = 0);
    }
}
