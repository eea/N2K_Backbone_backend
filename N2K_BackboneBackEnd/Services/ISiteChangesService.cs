using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeViewModel>> GetSiteChangesAsync();
        Task<SiteChangeDb> GetSiteChangeByIdAsync(int id);

        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();
    }
}
