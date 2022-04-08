using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChange>> GetSiteChangesAsync();
        Task<SiteChange> GetSiteChangeByIdAsync(int id);

        List<SiteChangeExtended> GetSiteChangesFromSP();
    }
}
