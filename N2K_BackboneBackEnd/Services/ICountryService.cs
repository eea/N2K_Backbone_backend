using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public interface ICountryService
    {
        Task<List<CountriesWithDataView>> GetWithDataAsync();
        Task<List<Countries>> GetAsync();
        Task<List<CountriesWithDataView>> GetWithDataAsync(SiteChangeStatus? status, Level? level);
        Task<List<CountriesChangesView>> GetPendingLevelAsync();
        Task<List<SitesWithChangesView>> GetSiteLevelAsync(SiteChangeStatus? status);
        Task<List<CountriesSiteCountView>> GetSiteCountAsync();
        Task<List<CountriesChangesView>> GetConsolidatedCountries();
        Task<List<ClosedCountriesView>> GetClosedAndDiscardedCountriesAsync();
    }
}