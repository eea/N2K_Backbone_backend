using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;


namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeDb>> GetSiteChangesAsync();
        Task<SiteChangeDb> GetSiteChangeByIdAsync(int id);

        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();
    }
}
