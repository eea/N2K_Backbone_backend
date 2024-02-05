using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
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

        [HttpGet("GetOverview")]
        public async Task<ActionResult<List<LineageCountry>>> GetOverview()
        {
            var response = new ServiceResponse<List<LineageCountry>>();
            try
            {
                var siteChanges = await _siteLineageService.GetOverview();
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
                response.Data = new List<LineageCountry>();
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

        [HttpGet("GetCodesCount")]
        public async Task<ActionResult<LineageCount>> GetCodesCount(string country, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            var response = new ServiceResponse<LineageCount>();
            try
            {
                var siteChanges = await _siteLineageService.GetCodesCount(country, _cache, creation, deletion, split, merge, recode);
                response.Data = siteChanges;
                response.Success = true;
                response.Message = "";
                response.Count = (siteChanges == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new LineageCount();
                return Ok(response);
            }
        }

        [Route("SaveEdition")]
        [HttpPost]
        public async Task<ActionResult<long>> SaveEdition(LineageConsolidation consolidateChanges)
        {
            var response = new ServiceResponse<long>();
            try
            {
                var siteChanges = await _siteLineageService.SaveEdition(consolidateChanges);
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count = 1; // siteChanges != null ? 1 : 0;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = -1;
                return Ok(response);
            }
        }

        [HttpGet("GetPredecessorsInfo")]
        public async Task<ActionResult<List<LineageEditionInfo>>> GetPredecessorsInfo(long ChangeId)
        {
            var response = new ServiceResponse<List<LineageEditionInfo>>();
            try
            {
                var siteChanges = await _siteLineageService.GetPredecessorsInfo(ChangeId);
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
                response.Data = new List<LineageEditionInfo>();
                return Ok(response);
            }
        }

        [HttpGet("GetLineageChangesInfo")]
        public async Task<ActionResult<LineageEditionInfo>> GetLineageChangesInfo(long ChangeId)
        {
            var response = new ServiceResponse<LineageEditionInfo>();
            try
            {
                var siteChanges = await _siteLineageService.GetLineageChangesInfo(ChangeId);
                response.Data = siteChanges;
                response.Success = true;
                response.Message = "";
                response.Count = 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }

        [HttpGet("GetLineageReferenceSites")]
        public async Task<ActionResult<List<SiteBasic>>> GetLineageReferenceSites(string country)
        {
            var response = new ServiceResponse<List<SiteBasic>>();
            try
            {
                var siteChanges = await _siteLineageService.GetLineageReferenceSites(country);
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
                response.Data = new List<SiteBasic>();
                return Ok(response);
            }
        }
    }
}