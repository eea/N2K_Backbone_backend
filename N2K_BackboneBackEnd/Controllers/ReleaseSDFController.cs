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
    public class ReleaseSDFController : ControllerBase
    {
        private readonly IReleaseSDFService _ReleaseSDFService;
        private readonly IMapper _mapper;

        public ReleaseSDFController(IReleaseSDFService ReleaseSDFService, IMapper mapper)
        {
            _ReleaseSDFService = ReleaseSDFService;
            _mapper = mapper;
        }

        [Route("GetData")]
        [HttpGet]
        public async Task<ActionResult<ReleaseSDF>> GetData(string SiteCode, int ReleaseId = -1)
        {
            ServiceResponse<ReleaseSDF> response = new();
            try
            {
                ReleaseSDF result = await _ReleaseSDFService.GetData(SiteCode, ReleaseId);
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
