using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataService masterDataService, IMapper mapper)
        {
            _masterDataService = masterDataService;
            _mapper = mapper;
        }

        [Route("MasterData/GetBioRegionTypes")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<BioRegionTypes>>>> GetBioRegionTypes()
        {
            var response = new ServiceResponse<List<BioRegionTypes>>();
            try
            {
                var bioRegionTypes = await _masterDataService.GetBioRegionTypes();
                response.Success = true;
                response.Message = "";
                response.Data = bioRegionTypes;
                response.Count = (bioRegionTypes == null) ? 0 : bioRegionTypes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<BioRegionTypes>();
                return Ok(response);
            }
        }

        [Route("MasterData/GetSiteTypes")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<SiteTypes>>>> GetSiteTypes()
        {
            var response = new ServiceResponse<List<SiteTypes>>();
            try
            {
                var siteTypes = await _masterDataService.GetSiteTypes();
                response.Success = true;
                response.Message = "";
                response.Data = siteTypes;
                response.Count = (siteTypes == null) ? 0 : siteTypes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<SiteTypes>();
                return Ok(response);
            }
        }
    }
}