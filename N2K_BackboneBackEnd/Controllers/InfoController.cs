using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IInfoService _infoService;
        private readonly IMapper _mapper;

        public InfoController(IInfoService controllerInfo, IMapper mapper)
        {
            _infoService = controllerInfo;
            _mapper = mapper;
        }

        [HttpGet("GetOfficialReleases")]
        public async Task<ActionResult<ServiceResponse<List<ReleasesCatalog>>>> GetOfficialReleases()
        {
            var response = new ServiceResponse<List<ReleasesCatalog>>();
            try
            {
                var data = await _infoService.GetOfficialReleases();
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : data.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<ReleasesCatalog>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredSites")]
        public async Task<ActionResult<ServiceResponse<List<SitesParametered>>>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? siteCode, string? siteName, string? habitatCode, string? speciesCode)
        {
            var response = new ServiceResponse<List<SitesParametered>>();
            try
            {
                var data = await _infoService.GetParameteredSites(releaseId, siteType, country, bioregion, siteCode, siteName, habitatCode, speciesCode);
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : data.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<SitesParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredHabitats")]
        public async Task<ActionResult<ServiceResponse<List<HabitatsParametered>>>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitatCode, string? habitatName)
        {
            var response = new ServiceResponse<List<HabitatsParametered>>();
            try
            {
                var data = await _infoService.GetParameteredHabitats(releaseId, habitatGroup, country, bioregion, habitatCode, habitatName);
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : data.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<HabitatsParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredSpecies")]
        public async Task<ActionResult<ServiceResponse<List<SpeciesParametered>>>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? speciesCode, string? speciesName)
        {
            var response = new ServiceResponse<List<SpeciesParametered>>();
            try
            {
                var data = await _infoService.GetParameteredSpecies(releaseId, speciesGroup, country, bioregion, speciesCode, speciesName);
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : data.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<SpeciesParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetLatestReleaseCounters")]
        public async Task<ActionResult<ServiceResponse<ReleaseCounters>>> GetLatestReleaseCounters()
        {
            var response = new ServiceResponse<ReleaseCounters>();
            try
            {
                var data = await _infoService.GetLatestReleaseCounters();
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new ReleaseCounters();
                return Ok(response);
            }
        }
    }
}