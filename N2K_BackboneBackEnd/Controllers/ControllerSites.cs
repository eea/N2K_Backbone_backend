using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerSites : ControllerBase
    {
        private readonly IControllerSite _controllerSiteChanges;
        private readonly IMapper _mapper;


        public ControllerSites(IControllerSite controllerSiteChanges, IMapper mapper)
        {
            _controllerSiteChanges = controllerSiteChanges;
            _mapper = mapper;
        }


        [HttpGet("Get")]
        public async Task<ActionResult<ServiceResponse<List<CountryNameCode>>>> GetCountriesWithData()
        {
            var response = new ServiceResponse<List<CountryNameCode>>();
            try
            {
                var countriesWithData = await _controllerSiteChanges.GetCountriesWithData();
                response.Success = true;
                response.Message = "";
                response.Data = countriesWithData;
                response.Count = (countriesWithData == null) ? 0 : countriesWithData.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<CountryNameCode>();
                return Ok(response);
            }
        }
    }
}
