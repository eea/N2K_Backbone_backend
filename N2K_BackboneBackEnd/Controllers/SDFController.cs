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

        [Route("GetData")]
        [HttpGet]
        public async Task<ActionResult<SDF>> GetData(string SiteCode)
        {
            ServiceResponse<SDF> response = new();
            try
            {
                SDF result = await _SDFService.GetData(SiteCode);
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

        [Route("GetData/SiteCode={SiteCode:string}&Version={Version:int}")]
        [HttpGet]
        public async Task<ActionResult<SDF>> GetData(string SiteCode, int Version)
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
    }
}