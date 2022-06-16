using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HarvestingController : ControllerBase
    {

        private readonly IHarvestedService _harvestedService;
        private readonly IMapper _mapper;

        public HarvestingController(IHarvestedService harvestedService, IMapper mapper)
        {
            _harvestedService = harvestedService;
            _mapper = mapper;
        }



        // GET: api/<HarvestingController>
        [Route("Get")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Harvesting>>>> Get()
        {
            var response = new ServiceResponse<List<Harvesting>>();
            try
            {
                var harvesting = await _harvestedService.GetHarvestedAsync();
                response.Success = true;
                response.Message = "";
                response.Data = harvesting;
                response.Count = (harvesting == null) ? 0 : harvesting.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Harvesting>();
                return Ok(response);
            }
        }



        //Id=>envelopeID
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<ServiceResponse<Harvesting>>> Get(int id)
        {
            var response = new ServiceResponse<Harvesting>();

            try
            {
                var harvesting = await _harvestedService.GetHarvestedAsyncById(id);
                response.Success = true;
                response.Message = "";
                response.Data = harvesting;
                response.Count = (harvesting == null) ? 0 : 1;
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


        [HttpGet]
        [Route("Harvested")]
        public IEnumerable<string> Harvested()
        {
            return new string[] { "value1", "value2" };
        }
        
        
        [HttpGet("Harvested/{fromDate}")]
        public IEnumerable<string> Harvested(DateTime? fromDate)
        {
            if (!fromDate.HasValue) return Harvested();
            return new string[] { "value1", "value2" };
        }

        [HttpGet("Harvested/{fromDate}/{toDate}")]
        public IEnumerable<string> Harvested(DateTime fromDate, DateTime? toDate)
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrives those envelopes with the status Pending
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Pending")]
        public async Task<ActionResult<ServiceResponse<Harvesting>>> Pending()
        {
            var response = new ServiceResponse<List<Harvesting>>();
            try
            {
                var pending = await _harvestedService.GetPendingEnvelopes();
                response.Success = true;
                response.Message = "";
                response.Data = pending;
                response.Count = (pending == null) ? 0 : pending.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Harvesting>();
                return Ok(response);
            }
        }

        /// <summary>
        /// Retrives those envelopes with the status Pending for the selected country
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("PendingByCountry/{CountryCode}")]
        public IEnumerable<string> PendingByCountry(string CountryCode)
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Executes the process of the harvesting for a selected envelop (Country and Version)
        /// </summary>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("Harvest")]
        [HttpPost]
        public async Task<ActionResult<List<HarvestedEnvelope>>> Harvest([FromBody] EnvelopesToProcess[] envelopes)
        {
            var response = new ServiceResponse<List<HarvestedEnvelope>>();
            try
            {
                var siteChanges = await _harvestedService.Harvest(envelopes);
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
                response.Data = new List<HarvestedEnvelope>();
                return Ok(response);
            }
        }

        /// <summary>
        /// Executes the process of the validation for a selected envelop (Country and Version).
        /// It must be hervested yet to perform this action
        /// </summary>
        /// <param name="envelopes"></param>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("Harvest/Validate")]
        [HttpPost]
        public async  Task<ActionResult<List<HarvestedEnvelope>>> Validate([FromBody] EnvelopesToProcess[] envelopes)
        {
            var response = new ServiceResponse<List<HarvestedEnvelope>>();
            try
            {
                var processedEnvelope = await _harvestedService.Validate(envelopes);
                response.Success = true;
                response.Message = "";
                response.Data = processedEnvelope;
                response.Count = (processedEnvelope == null) ? 0 : processedEnvelope.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<HarvestedEnvelope>();
                return Ok(response);
            }
        }

        /// <summary>
        /// Executes the process of the validation for a selected site (Sitecode and Version).
        /// It must be hervested yet to perform this action
        /// </summary>
        /// <param name="envelopes"></param>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("Harvest/ValidateSingleSite")]
        [HttpPost]
        public async Task<ActionResult<List<HarvestedEnvelope>>> ValidateSingleSite([FromBody] string siteCode, int versionId)
        {
            var response = new ServiceResponse<List<HarvestedEnvelope>>();
            try
            {
                var processedEnvelope = await _harvestedService.ValidateSingleSite(siteCode, versionId);
                response.Success = true;
                response.Message = "";
                response.Data = processedEnvelope;
                response.Count = (processedEnvelope == null) ? 0 : processedEnvelope.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<HarvestedEnvelope>();
                return Ok(response);
            }
        }
    }
}
