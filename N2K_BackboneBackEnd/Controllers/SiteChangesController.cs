using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;

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


        [Route("Get")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> Get()
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync();
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count= (siteChanges== null)?0:siteChanges.Count;
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


        [Route("GetByStatus")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeDb>>>> GetByStatus(SiteChangeStatus status)
        {
            var response = new ServiceResponse<List<SiteChangeDb>>();
            try
            {
                var siteChanges = await _siteChangesService.GetSiteChangesAsync(status);
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


        [HttpGet("GetSiteChangesDetail/siteCode={pSiteCode}&version={pCountryVersion}")            ]
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

        [HttpGet("GetSiteChangesDetailExtended/siteCode={pSiteCode}&version={pCountryVersion}")]
        /// <summary>
        /// Remove the version we use in development
        /// </summary>
        /// <param name="pSiteCode">Code of the site</param>
        /// <param name="pCountryVersion">Number of the version</param>
        public async Task<ActionResult<ServiceResponse<SiteChangeDetailViewModelAdvanced>>> GetSiteChangesDetailExtended(string pSiteCode, int pCountryVersion)
        {
            var response = new ServiceResponse<SiteChangeDetailViewModelAdvanced>();

            try
            {
                var siteChange = await _siteChangesService.GetSiteChangesDetailExtended(pSiteCode, pCountryVersion);
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
