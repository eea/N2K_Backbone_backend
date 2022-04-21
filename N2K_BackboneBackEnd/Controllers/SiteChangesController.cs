using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;

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
        public async Task<ActionResult<ServiceResponse<List<SiteChange>>>> Get()
        {
            var response = new ServiceResponse<List<SiteChange>>();
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
                response.Data = new List<SiteChange>();
                return Ok(response);
            }
        }


        [Route("GetFromSP")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<SiteChangeExtended>>>> GetFromSP()
        {
            var response = new ServiceResponse<List<SiteChangeExtended>>();
            try
            {
                var siteChangesExt = await _siteChangesService.GetSiteChangesFromSP();
                response.Success = true;
                response.Message = "";
                response.Data = siteChangesExt;
                response.Count = (siteChangesExt == null) ? 0 : siteChangesExt.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<SiteChangeExtended>();
                return Ok(response);
            }
        }



        [HttpGet("Get/{id}")]
        public async Task<ActionResult<ServiceResponse<SiteChangeDb>>> Get(int id)
        {
            var response = new ServiceResponse<SiteChangeDb>();

            try
            {
                var siteChange = await _siteChangesService.GetSiteChangeByIdAsync(id);
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

        /*
        [HttpGet("Get/{id}")]
        public ActionResult<ServiceResponse<SiteChange>> Get(int id)
        {
            var response = new ServiceResponse<SiteChange>();
            try
            {
                var siteChange =  _siteChangesService.GetSiteChangeById(id);
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
        */
        

        /*
        // POST api/<SiteChangesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

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
