using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;

namespace N2K_BackboneBackEnd.Services
{
    public interface IControllerSiteService
    {
        Task<ActionResult<ServiceResponse<List<CountryNameCode>>>> GetCountriesWithDataAsync();
    }
}
