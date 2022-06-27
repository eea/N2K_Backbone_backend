using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public interface ICountryService
    {
        Task<List<Countries>> GetWithDataAsync();
        Task<List<Countries>> GetAsync();
        Task<List<Countries>> GetWithDataAsync(SiteChangeStatus? status, Level? level);
        Task<List<CountriesChangesView>> GetPendingLevelAsync();
        Task<List<CountriesSiteCountView>> GetSiteCountAsync();
    }
}
