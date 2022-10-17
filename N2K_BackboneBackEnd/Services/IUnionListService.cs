using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface IUnionListService
    {
        Task<List<BioRegionTypes>> GetUnionBioRegionTypes();
        Task<List<UnionListHeader>> GetUnionListHeadersByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListDetail>> GetCurrentSitesUnionListDetailByBioRegion(string? bioRegionShortCode);
        Task<List<UnionListHeader>> GetUnionListHeadersById(long? id);
        Task<List<UnionListComparerViewModel>> CompareUnionLists(long? idSource, long? idTarget);
        Task<List<UnionListHeader>> CreateUnionList(string name, Boolean final);
        Task<List<UnionListHeader>> UpdateUnionList(long id, string name, Boolean final);
        Task<int> DeleteUnionList(long id);
    }
}
