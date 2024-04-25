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
    public class SiteChangesController : ControllerBase
    {
        private readonly ISiteChangesService _siteChangesService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;

        public SiteChangesController(ISiteChangesService siteChangesService, IMapper mapper, IMemoryCache cache)
        {
            _siteChangesService = siteChangesService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet("Get")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> Get()
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, null, null, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetPaginated(int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty, null, null, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountry(string country)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, null, null, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryPaginated(string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, null, null, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/level={level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByLevel(Level? level)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty, null, level, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/level={level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByLevelPaginated(Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, null, level, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }


        [HttpGet("Get/country={country:string}&level={level:Level}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndLevel(string country, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, null, level, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&level={level:Level}&page={page:int}&limit={limit:int}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByLevelAndCountryPaginated(string country, Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, null, level, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/status={status:Status}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByStatus(SiteChangeStatus? status)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty, status, null, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/status={status:Status}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByStatusPaginated(SiteChangeStatus status, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, status, null, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndStatus(string country, SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, status, null, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndStatusPaginated(string country, SiteChangeStatus status, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, status, null, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/status={status}&level={level:Level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByStatusAndLevel(SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, status, level, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/status={status}&level={level:Level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByStatusAndLevelPaginated(SiteChangeStatus status, Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty, status, level, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&level={level:Level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndStatusAndLevel(string country, SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, status, level, _cache);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&level={level:Level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndStatusAndLevelPaginated(string country, SiteChangeStatus status, Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, status, level, _cache, page, limit);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&level={level:Level}&page={page:int}&limit={limit:int}&onlyedited={onlyedited:bool}&onlyjustreq={onlyjustreq:bool}&onlysci={onlysci:bool}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDbEdition>>>> GetByCountryAndStatusAndLevelPaginated(string country, SiteChangeStatus status, Level level, int page, int limit, bool onlyedited, bool onlyjustreq, bool onlysci)
        {
            var response = new ServiceResponse<List<SiteChangeDbEdition>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, status, level, _cache, page, limit, onlyedited, onlyjustreq, onlysci);
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
                response.Data = new List<SiteChangeDbEdition>();
                return Ok(response);
            }
        }

        [Route("GetSiteCodes/country={country:string}&status={status:Status}&level={level:Level}&onlyedited={onlyedited:bool}&onlyjustreq={onlyjustreq:bool}&onlysci={onlysci:bool}/")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, bool onlyedited, bool onlyjustreq, bool onlysci)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(country, status, level, _cache, false, onlyedited, onlyjustreq, onlysci);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = await _siteChangesService.GetPendingChangesByCountry(country, _cache);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                return Ok(response);
            }
        }

        [Route("GetNonPendingSiteCodes/country={country:string}&onlyedited={onlyedited:bool}&onlyjustreq={onlyjustreq:bool}&onlysci={onlysci:bool}/")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetNonPendingSiteCodes(string country, Boolean onlyedited, Boolean onlyjustreq, Boolean onlysci)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetNonPendingSiteCodes(country, onlyedited, onlyjustreq, onlysci);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = await _siteChangesService.GetPendingChangesByCountry(country, _cache);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                return Ok(response);
            }
        }

        [HttpGet("GetSiteChangesDetail/siteCode={pSiteCode}&version={pCountryVersion}")]
        /// <summary>
        /// Remove the version we use in development
        /// </summary>
        /// <param name="pSiteCode">Code of the site</param>
        /// <param name="pCountryVersion">Number of the version</param>
        public async Task<ActionResult<ServiceResponse<SiteChangeDetailViewModel>>> GetSiteChangesDetail(string pSiteCode, int pCountryVersion)
        {
            var response = new ServiceResponse<SiteChangeDetailViewModel>();
            try
            {
                var siteChange = await _siteChangesService.GetSiteChangesDetail(pSiteCode, pCountryVersion);
                response.Success = true;
                response.Message = "";
                response.Data = siteChange;
                response.Count = (siteChange == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                return Ok(response);
            }
        }

        // POST api/<SiteChangesController>
        [Route("AcceptChanges/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> AcceptChanges([FromBody] ModifiedSiteCode[] acceptedChanges)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.AcceptChanges(acceptedChanges, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        // POST api/<SiteChangesController>
        [Route("AcceptChangesBulk/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> AcceptChangesBulk(string sitecodes)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.AcceptChangesBulk(sitecodes, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        // POST api/<SiteChangesController>
        [Route("MoveToPending/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> MoveToPending([FromBody] ModifiedSiteCode[] changedSiteStatus)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.MoveToPending(changedSiteStatus, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        // POST api/<SiteChangesController>
        [Route("MoveToPendingBulk/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> MoveToPendingBulk(string sitecodes)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.MoveToPendingBulk(sitecodes, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        [Route("RejectChanges/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> RejectChanges([FromBody] ModifiedSiteCode[] rejectedChanges)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.RejectChanges(rejectedChanges, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        [Route("RejectChangesBulk/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> RejectChangesBulk(string sitecodes)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.RejectChangesBulk(sitecodes, _cache);
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
                response.Data = new List<ModifiedSiteCode>();
                return Ok(response);
            }
        }

        [Route("MarkAsJustificationRequired/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> MarkAsJustificationRequired([FromBody] JustificationModel[] sitesToMarkAsJustified)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.MarkAsJustificationRequired(sitesToMarkAsJustified, _cache);
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
                response.Data = null;
                return Ok(response);
            }
        }

        [Route("ProvideJustification/")]
        [HttpPost]
        public async Task<ActionResult<List<ModifiedSiteCode>>> JustificationProvided([FromBody] JustificationModel[] sitesToProvideJustification)
        {
            var response = new ServiceResponse<List<ModifiedSiteCode>>();
            try
            {
                var siteChanges = await _siteChangesService.JustificationProvided(sitesToProvideJustification);
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
                response.Data = null;
                return Ok(response);
            }
        }

        [HttpGet("GetNoChanges/country={country:string}")]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeVersion>>>> GetNoChanges(string country)
        {
            var response = new ServiceResponse<List<SiteCodeVersion>>();
            try
            {
                var siteChanges = await _siteChangesService.GetNoChanges(country, _cache);
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
                response.Data = new List<SiteCodeVersion>();
                return Ok(response);
            }
        }

        [HttpGet("GetNoChanges/country={country:string}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeVersion>>>> GetNoChanges(string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteCodeVersion>>();
            try
            {
                var siteChanges = await _siteChangesService.GetNoChanges(country, _cache, page, limit);
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
                response.Data = new List<SiteCodeVersion>();
                return Ok(response);
            }
        }

        [HttpGet("GetPendingVersion/siteCode={siteCode:string}")]
        public async Task<ActionResult<ServiceResponse<int>>> GetPendingVersion(string siteCode)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var siteChanges = await _siteChangesService.GetPendingVersion(siteCode);
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count = (siteChanges == null) ? 0 : 1;
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

        /*
        // PUT api/<SiteChangesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SiteChangesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        
        private async Task<bool> Save()
        {
            return await _dataContext.SaveChangesAsync() >= 0 ? true : false;
        }
        */
    }
}
