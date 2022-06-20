using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;

namespace N2K_BackboneBackEnd.Services
{
    public interface ICountryService
    {
        Task<List<Countries>> GetCountriesWithDataAsync();
        Task<List<Countries>> GetCountriesAsync();
    }
}
