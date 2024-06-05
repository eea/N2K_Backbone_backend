using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class SDFController : ControllerBase
    {
        private readonly ISDFService _SDFService;
        private readonly IMapper _mapper;

        public SDFController(ISDFService SDFService, IMapper mapper)
        {
            _SDFService = SDFService;
            _mapper = mapper;
        }

        [Route("GetExtraData")]
        [HttpGet]
        public async Task<ActionResult<SDF>> GetExtraData(string SiteCode, int submission)
        {
            ServiceResponse<SDF> response = new();
            try
            {
                SDF result = await _SDFService.GetExtraData(SiteCode, submission);
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
                response.Data = new SDF();
                return Ok(response);
            }
        }

        [Route("GetData")]
        [HttpGet]
        public async Task<ActionResult<SDF>> GetData(string SiteCode, int Version = -1)
        {
            ServiceResponse<SDF> response = new();
            try
            {
                SDF result = await _SDFService.GetData(SiteCode, Version);
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
                response.Data = new SDF();
                return Ok(response);
            }
        }

        [Route("GetReleaseData")]
        [HttpGet]
        public async Task<ActionResult<ReleaseSDF>> GetReleaseData(string SiteCode, int ReleaseId = -1)
        {
            ServiceResponse<ReleaseSDF> response = new();
            try
            {
                ReleaseSDF result = await _SDFService.GetReleaseData(SiteCode, ReleaseId);
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
    
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class PublicSDFController : ControllerBase
    {
        private readonly ISDFService _SDFService;
        private readonly IMapper _mapper;

        public PublicSDFController(ISDFService SDFService, IMapper mapper)
        {
            _SDFService = SDFService;
            _mapper = mapper;
        }
        
        [AllowAnonymous]
        [Route("GetPublicReleaseData")]
        [HttpGet]
        public async Task<ActionResult<ReleaseSDF>> GetReleaseData(string SiteCode, int ReleaseId = -1)
        {
            ServiceResponse<ReleaseSDF> response = new();
            try
            {
                ReleaseSDF result = await _SDFService.GetReleaseData(SiteCode, ReleaseId, false);
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
