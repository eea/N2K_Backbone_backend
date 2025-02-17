using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

namespace N2K_BackboneBackEnd.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class PublicDataController : ControllerBase
    {
        private readonly ISDFService _SDFService;
        private readonly IMapper _mapper;

        public PublicDataController(ISDFService SDFService, IMapper mapper)
        {
            _SDFService = SDFService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [Route("GetReleaseData")]
        [HttpGet]
        public async Task<ActionResult<ReleaseSDF>> GetReleaseData(string SiteCode, int ReleaseId = -1, Boolean initialValidation = false, Boolean internalViewers = false, Boolean internalBarometer = false, Boolean internalPortalSDFSensitive = false, Boolean publicViewers = false, Boolean publicBarometer = false, Boolean sdfPublic = false, Boolean naturaOnlineList = false, Boolean productsCreated = false, Boolean jediDimensionCreatedA = false)
        {
            ServiceResponse<ReleaseSDF> response = new();
            try
            {
                ReleaseSDF result = await _SDFService.GetReleaseData(SiteCode, ReleaseId, initialValidation, internalViewers, internalBarometer, internalPortalSDFSensitive, publicViewers, publicBarometer, sdfPublic, naturaOnlineList, productsCreated, jediDimensionCreatedA);
                response.Success = true;
                response.Message = "";
                response.Data = result;
                response.Count = 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new ReleaseSDF();
                return Ok(response);
            }
        }

    }
}
