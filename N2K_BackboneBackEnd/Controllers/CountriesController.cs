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
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;


        public CountriesController(ICountryService controllerSiteChanges, IMapper mapper)
        {
            _countryService = controllerSiteChanges;
            _mapper = mapper;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<ServiceResponse<List<Countries>>>> Get()
        {
            var response = new ServiceResponse<List<Countries>>();
            try
            {
                var countriesWithData = await _countryService.GetAsync();
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
                response.Data = new List<Countries>();
                return Ok(response);
            }
        }

        [HttpGet("GetWithData")]
        public async Task<ActionResult<ServiceResponse<List<Countries>>>> GetWithData(SiteChangeStatus? status, Level? level)
        {
            var response = new ServiceResponse<List<Countries>>();
            try
            {
                var countriesWithData = await _countryService.GetWithDataAsync(status, level);
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
                response.Data = new List<Countries>();
                return Ok(response);
            }
        }
    }
}
