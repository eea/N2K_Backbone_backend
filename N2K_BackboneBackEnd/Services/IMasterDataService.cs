using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface IMasterDataService
    {
        Task<List<BioRegionTypes>> GetBioRegionTypes();

        Task<List<SiteTypes>> GetSiteTypes();
    }
}
