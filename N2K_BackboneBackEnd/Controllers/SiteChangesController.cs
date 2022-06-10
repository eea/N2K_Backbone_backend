using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteChangesController : ControllerBase
    {
        private readonly ISiteChangesService _siteChangesService;
        private readonly IMapper _mapper;


        public SiteChangesController(ISiteChangesService siteChangesService, IMapper mapper)
        {
            _siteChangesService = siteChangesService;
            _mapper = mapper;
        }


        [HttpGet("Get")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> Get()
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, null, null);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }
        [HttpGet("Get/page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetPaginated(int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty,null, null, page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountry(string country)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country, null, null);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/country={country:string}&page={page:int}&limit={limit:int}")]        
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryPaginated(string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,null, null, page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/level={level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByLevel(Level? level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty,null, level);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }
        
        [HttpGet("Get/level={level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByLevelPaginated(Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, null, level, page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/country={country:string}&level={level:Level}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryAndLevel(string country,Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,null, level);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/country={country:string}&level={level:Level}&page={page:int}&limit={limit:int}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByLevelAndCountryPaginated(string country,Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,null, level,page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/status={status:Status}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatus(SiteChangeStatus? status)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty,status, null);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/status={status:Status}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusPaginated(SiteChangeStatus status, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty,status, null,  page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryAndStatus(string country,SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,status, null);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryAndStatusPaginated(string country, SiteChangeStatus status, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,status, null, page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/status={status}&level={level:Level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevel(SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(String.Empty, status, level);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [HttpGet("Get/status={status}&level={level:Level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevelPaginated(SiteChangeStatus status, Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(string.Empty,status, level,page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&level={level:Level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryAndStatusAndLevel(string country,SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,status, level);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }

        [HttpGet("Get/country={country:string}&status={status:Status}&level={level:Level}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByCountryAndStatusAndLevelPaginated(string country,SiteChangeStatus status, Level level,int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(country,status, level, page, limit);
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
                response.Data = new List<SiteChangeDb>();
                return Ok(response);
            }
        }


        [Route("GetSiteCodes/level={level}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByLevel(Level level)
        {
            return await GetSiteCodesByLevelAndCountry(string.Empty,level);
        }


        [Route("GetSiteCodes/country={country:string}&level={level:Level}/")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByLevelAndCountry(string country,Level level)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(country,null, level);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = (siteCodes == null) ? 0 : siteCodes.Count;
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


        [Route("GetSiteCodes/status={status}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatus(SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(String.Empty, status, null);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = (siteCodes == null) ? 0 : siteCodes.Count;
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


        [Route("GetSiteCodes/country={country:string}&status={status:Status}/")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatus(string country,SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(country,status, null);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = (siteCodes == null) ? 0 : siteCodes.Count;
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

        [Route("GetSiteCodes/status={status}&level={level:Level}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatusAndLevel(SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(String.Empty, status, level);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = (siteCodes == null) ? 0 : siteCodes.Count;
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



        [Route("GetSiteCodes/country={country:string}&status={status:Status}&level={level:Level}/")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatusAndLevelAndCountry(string country,SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(country,status, level);
                response.Success = true;
                response.Message = "";
                response.Data = siteCodes;
                response.Count = (siteCodes == null) ? 0 : siteCodes.Count;
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
                var siteChanges = await _siteChangesService.AcceptChanges(acceptedChanges);
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
                var siteChanges = await _siteChangesService.RejectChanges(rejectedChanges);
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
        public async Task<ActionResult<int>> MarkAsJustificationRequired([FromBody] ModifiedSiteCode[] siteToMarkAsJustified)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var siteChanges = await _siteChangesService.MarKAsJustificationRequired(siteToMarkAsJustified);
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
                response.Data = 0;
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
