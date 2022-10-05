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
        Task<List<UnionListDetail>> CompareUnionLists(long? idTarget, long? idSource);
        Task<UnionListHeader> CreateUnionList(string name, Boolean final);
        Task<UnionListHeader> EditUnionList(long id, string name, Boolean final);
        Task<int> DeleteUnionList(long id);
    }
}
