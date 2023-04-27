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


        public SiteLineageController(ISiteLineageService siteLineageService, IMapper mapper, IMemoryCache cache)
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


        [HttpGet("GetChanges")]
        public async Task<ActionResult<List<LineageChanges>>> GetChanges(string country, LineageStatus status, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            var response = new ServiceResponse<List<LineageChanges>>();
            try
            {
                var siteChanges = await _siteLineageService.GetChanges(country, status, _cache, page, pageLimit, creation, deletion, split, merge, recode);
                response.Data = siteChanges;
                response.Success = true;
                response.Message = "";
                response.Count = (siteChanges == null) ? 0 : siteChanges.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<LineageChanges>();
                return Ok(response);
            }

        }


        [Route("ConsolidateChanges")]
        [HttpPost]
        public async Task<ActionResult<List<LineageConsolidate>>> ConsolidateChanges(List<LineageConsolidate> consolidateChanges)
        {
            var response = new ServiceResponse<List<LineageConsolidate>>();
            try
            {
                var siteChanges = await _siteLineageService.ConsolidateChanges(consolidateChanges);
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count = (siteChanges == null) ? 0 : siteChanges.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<LineageConsolidate>();
                return Ok(response);
            }
        }

        //// POST api/<SiteChangesController>
        [Route("SetChangesBackToPropose/")]
        [HttpPost]
        public async Task<ActionResult<List<Lineage>>> SetChangesBackToPropose(List<Lineage> changeId)
        {
            var response = new ServiceResponse<List<Lineage>>();
            try
            {
                var siteChanges = await _siteLineageService.SetChangesBackToPropose(changeId);
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count = (siteChanges == null) ? 0 : siteChanges.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Lineage>();
                return Ok(response);
            }
        }
    }
}
