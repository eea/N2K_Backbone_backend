using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.AspNetCore.Authorization;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
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
        public async Task<ActionResult<ServiceResponse<List<CountriesWithDataView>>>> GetWithData(SiteChangeStatus? status, Level? level)
        {
            var response = new ServiceResponse<List<CountriesWithDataView>>();
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
                response.Data = new List<CountriesWithDataView>();
                return Ok(response);
            }
        }

        [HttpGet("GetPendingLevel")]
        public async Task<ActionResult<ServiceResponse<List<CountriesChangesView>>>> GetPendingLevel()
        {
            var response = new ServiceResponse<List<CountriesChangesView>>();
            try
            {
                var countriesWithData = await _countryService.GetPendingLevelAsync();
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
                response.Data = new List<CountriesChangesView>();
                return Ok(response);
            }
        }

        [HttpGet("GetConsolidatedCountries")]
        public async Task<ActionResult<ServiceResponse<List<CountriesChangesView>>>> GetConsolidatedCountries()
        {
            var response = new ServiceResponse<List<CountriesChangesView>>();
            try
            {
                List<CountriesChangesView> countriesWithData = await _countryService.GetConsolidatedCountries();
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
                response.Data = new List<CountriesChangesView>();
                return Ok(response);
            }
        }

        [HttpGet("GetClosedAndDiscardedCountries")]
        public async Task<ActionResult<ServiceResponse<List<ClosedCountriesView>>>> GetClosedAndDiscardedCountries()
        {
            var response = new ServiceResponse<List<ClosedCountriesView>>();
            try
            {
                List<ClosedCountriesView> countriesWithData = await _countryService.GetClosedAndDiscardedCountriesAsync();
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
                response.Data = new List<ClosedCountriesView>();
                return Ok(response);
            }
        }

        [HttpGet("GetSiteCount")]
        public async Task<ActionResult<ServiceResponse<List<CountriesSiteCountView>>>> GetSiteCount()
        {
            var response = new ServiceResponse<List<CountriesSiteCountView>>();
            try
            {
                var countriesWithData = await _countryService.GetSiteCountAsync();
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
                response.Data = new List<CountriesSiteCountView>();
                return Ok(response);
            }
        }

        [HttpGet("GetSiteLevel")]
        public async Task<ActionResult<ServiceResponse<List<SitesWithChangesView>>>> GetSiteLevel(SiteChangeStatus? status)
        {
            var response = new ServiceResponse<List<SitesWithChangesView>>();
            try
            {
                var countriesWithData = await _countryService.GetSiteLevelAsync(status);
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
                response.Data = new List<SitesWithChangesView>();
                return Ok(response);
            }
        }

        [HttpGet("GetEditionCountries")]
        public async Task<ActionResult<ServiceResponse<List<EditionCountriesCountViewModel>>>> GetEditionCountries()
        {
            var response = new ServiceResponse<List<EditionCountriesCountViewModel>>();
            try
            {
                var countriesEdition = await _countryService.GetEditionCountries();
                response.Success = true;
                response.Message = "";
                response.Data = countriesEdition;
                response.Count = (countriesEdition == null) ? 0 : countriesEdition.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<EditionCountriesCountViewModel>();
                return Ok(response);
            }
        }
    }
}