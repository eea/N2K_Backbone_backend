using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using N2K_BackboneBackEnd.Models;
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
    public class SiteLineageController : ControllerBase
    {
        private readonly ISiteLineageService _siteLineageService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;


        public SiteLineageController(ISiteLineageService siteLineageService, IMapper mapper,IMemoryCache cache)
        {
            _siteLineageService = siteLineageService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }


        [HttpGet("GetSiteLineage")]
        public async Task<ActionResult<ServiceResponse<List<SiteLineage>>>> GetSiteLineage(string siteCode)
        {
            var response = new ServiceResponse<List<SiteLineage>>();
            try
            {
                var siteLineage = await _siteLineageService.GetSiteLineageAsync(siteCode);
                response.Success = true;
                response.Message = "";
                response.Data = siteLineage;
                response.Count = (siteLineage == null) ? 0 : siteLineage.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<SiteLineage>();
                return Ok(response);
            }
        }
    }
}
