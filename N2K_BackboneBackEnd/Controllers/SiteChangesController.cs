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
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, null);
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
        [HttpGet("Get/page={page}&limit={limit}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetPaginated(int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, null, string.Empty, page, limit);
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

        [HttpGet("Get/{country}/page={page}&limit={limit}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetPaginatedByCountry(string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, null, country, page, limit);
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

        [HttpGet("Get/{level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByLevel(Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, level);
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

        [HttpGet("Get/{level}/page={page}&limit={limit}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByLevelPaginated(Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, level, string.Empty, page, limit);
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


        [HttpGet("Get/{level}/{country}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetPaginatedByLevelAndCountry(Level level, string country)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, null, country);
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


        [HttpGet("Get/{level}/{country}/page={page}&limit={limit}")]
        //[HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetPaginatedByLevelAndCountryPaginated(Level level, string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(null, null, country, page, limit);
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



        [HttpGet("Get/{status}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatus(SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, null);
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


        [HttpGet("Get/{status}/page={page}&limit={limit}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusPaginated(SiteChangeStatus status, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, null, String.Empty, page, limit);
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

        [HttpGet("Get/{status}/{country}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndCountry(SiteChangeStatus status, string country)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, null, country);
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

        [HttpGet("Get/{status}/{country}/page={page}&limit={limit}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndCountryPaginated(SiteChangeStatus status, string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, null, country, page, limit);
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


        [HttpGet("Get/{status}/{level}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevel(SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, level);
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


        [HttpGet("Get/{status}/{level}/page={page}&limit={limit}")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevelPaginated(SiteChangeStatus status, Level level, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, level, string.Empty, page, limit);
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

        [HttpGet("Get/{status}/{level}/{country}/")]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevelAndCountry(SiteChangeStatus status, Level level, string country)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, level, country);
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

        [Route("Get/{status}/{level}/{country}/page={page}&limit={limit}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatusAndLevelAndCountryPaginated(SiteChangeStatus status, Level level, string country, int page, int limit)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status, level, country, page, limit);
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





        [Route("GetSiteCodes/{level}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByLevel(Level level)
        {
            return await GetSiteCodesByLevelAndCountry(level, "");
        }


        [Route("GetSiteCodes/{level}/{country}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByLevelAndCountry(Level level, string country)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(null, level, country);
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


        [Route("GetSiteCodes/{status}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatus(SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(status, null);
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


        [Route("GetSiteCodes/{status}/{country}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatus(SiteChangeStatus status, string country)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(status, null, country);
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

        [Route("GetSiteCodes/{status}/{level}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatusAndLevel(SiteChangeStatus status, Level level)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(status, level);
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



        [Route("GetSiteCodes/{status}/{level}/{country}")]
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse<List<SiteCodeView>>>> GetSiteCodesByStatusAndLevelAndCountry(SiteChangeStatus status, Level level, string country)
        {
            var response = new ServiceResponse<List<SiteCodeView>>();
            try
            {
                var siteCodes = await _siteChangesService.GetSiteCodesByStatusAndLevelAndCountry(status, level, country);
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




        [HttpGet("GetSiteCodes/siteCode={pSiteCode}&version={pCountryVersion}")]
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
