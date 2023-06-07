using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Newtonsoft.Json;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class HarvestingController : ControllerBase
    {

        private readonly IHarvestedService _harvestedService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;
        //private readonly BackgroundWorkerQueue _backgroundWorkerQueue;
        private readonly IFireForgetRepositoryHandler _fireForgetRepositoryHandler;

        public HarvestingController(IHarvestedService harvestedService, IMapper mapper, IMemoryCache cache, IFireForgetRepositoryHandler fireForgetRepositoryHandler)
        {
            _harvestedService = harvestedService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _fireForgetRepositoryHandler = fireForgetRepositoryHandler;
            //_backgroundWorkerQueue = backgroundWorkerQueue;
        }



        /*
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
        */

        /*

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
        */

        /// <summary>
        /// Retrives those envelopes with the input status
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEnvelopesByStatus")]
        public async Task<ActionResult<ServiceResponse<HarvestingExpanded>>> GetEnvelopesByStatus(HarvestingStatus status)
        {
            var response = new ServiceResponse<List<HarvestingExpanded>>();
            try
            {
                var envelopes = await _harvestedService.GetEnvelopesByStatus(status);
                response.Success = true;
                response.Message = "";
                response.Data = envelopes;
                response.Count = (envelopes == null) ? 0 : envelopes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<HarvestingExpanded>();
                return Ok(response);
            }
        }

        /// <summary>
        /// Retrives contries with closed envelopes and no open envelopes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOnlyClosedEnvelopes")]
        public async Task<ActionResult<ServiceResponse<HarvestingExpanded>>> GetOnlyClosedEnvelopes()
        {
            var response = new ServiceResponse<List<HarvestingExpanded>>();
            try
            {
                var envelopes = await _harvestedService.GetOnlyClosedEnvelopes();
                response.Success = true;
                response.Message = "";
                response.Data = envelopes;
                response.Count = (envelopes == null) ? 0 : envelopes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<HarvestingExpanded>();
                return Ok(response);
            }
        }

        /*
        [HttpGet]
        [Route("Harvested")]
        public IEnumerable<string> Harvested()
        {
            return new string[] { "value1", "value2" };
        }
        */

        /*

        [HttpGet("Harvested/{fromDate}")]
        public IEnumerable<string> Harvested(DateTime? fromDate)
        {
            if (!fromDate.HasValue) return Harvested();
            return new string[] { "value1", "value2" };
        }
        */
        /*
        [HttpGet("Harvested/{fromDate}/{toDate}")]
        public IEnumerable<string> Harvested(DateTime fromDate, DateTime? toDate)
        {
            return new string[] { "value1", "value2" };
        }
        */

        /*
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
        */

        /*
        [HttpGet]
        [Route("PreHarvested")]
        public async Task<ActionResult<ServiceResponse<EnvelopesToHarvest>>> PendingToHarvest()
        {
            var response = new ServiceResponse<List<EnvelopesToHarvest>>();
            try
            {
                var pending = await _harvestedService.GetPreHarvestedEnvelopes();
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
                response.Data = new List<EnvelopesToHarvest>();
                return Ok(response);
            }
        }
        */

        /*
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
        */

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
        /// Execute an unattended load of the data from versioning
        /// </summary>
        /// <returns></returns>
        [Route("FullHarvest")]
        [HttpPost]
        public async Task<ActionResult<int>> FullHarvest()
        {
            var response = new ServiceResponse<int>();
            try
            {
                await Task.Delay(1);
                // Delegate the blog auditing to another task on the threadpool
                _fireForgetRepositoryHandler.Execute(async repository =>
                {
                    // Will receive its own scoped repository on the executing task
                    await repository.FullHarvest(_cache);
                });
                response.Success = true;
                response.Message = "";
                response.Data = 1;
                response.Count = 1;
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


        /// <summary>
        /// Executes the process of the harvesting for a selected envelop (Country and Version)
        /// </summary>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("HarvestSpatialData")]
        [HttpPost]
        public async Task<ActionResult<int>> HarvestSpatialData([FromBody] EnvelopesToProcess[] envelopes)
        {
            var response = new ServiceResponse<int>();
            try
            {
                await Task.Delay(1);
                // Delegate the blog auditing to another task on the threadpool
                _fireForgetRepositoryHandler.Execute(async repository =>
                {
                    // Will receive its own scoped repository on the executing task
                    await repository.HarvestSpatialData(envelopes, _cache);
                });

                response.Success = true;
                response.Message = "";
                response.Data = 1;
                response.Count = 1;
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



        /// <summary>
        /// Changes te status of a envelope
        /// </summary>
        /// <returns></returns>
        [Route("ChangeStatus")]
        [HttpPost]
        public async Task<ActionResult<ProcessedEnvelopes>> ChangeStatus(string country, int version, HarvestingStatus toStatus)
        {
            var response = new ServiceResponse<ProcessedEnvelopes>();
            try
            {
                var siteChanges = await _harvestedService.ChangeStatus(country, version, toStatus, _cache);
                response.Success = true;
                response.Message = "";
                response.Data = siteChanges;
                response.Count = 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new ProcessedEnvelopes();
                return  Ok(response);
            }
        }




        /// <summary>
        /// Executes the process of the ChangeDetection for a selected envelop (Country and Version).
        /// It must be hervested yet to perform this action
        /// </summary>
        /// <param name="envelopes"></param>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("Harvest/ChangeDetection")]
        [HttpPost]
        public async  Task<ActionResult<List<HarvestedEnvelope>>> ChangeDetection([FromBody] EnvelopesToProcess[] envelopes)
        {
            var response = new ServiceResponse<List<HarvestedEnvelope>>();
            try
            {
                var processedEnvelope = await _harvestedService.ChangeDetection(envelopes,null);
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
        /// Executes the process of the ChangeDetection for a selected site (Sitecode and Version).
        /// It must be hervested yet to perform this action
        /// </summary>
        /// <param name="envelopes"></param>
        /// <returns></returns>
        // POST api/<HarvestingController>
        [Route("Harvest/ChangeDetectionSingleSite")]
        [HttpPost]
        public async Task<ActionResult<List<HarvestedEnvelope>>> ChangeDetectionSingleSite(string siteCode, int versionId)
        {
            var response = new ServiceResponse<List<HarvestedEnvelope>>();
            try
            {
                var processedEnvelope = await _harvestedService.ChangeDetectionSingleSite(siteCode, versionId);
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

        [AllowAnonymous]
        [Route("/ws")]
        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private  async Task Echo(System.Net.WebSockets.WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                string msg = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                if (!string.IsNullOrEmpty(msg)) {
                    await _harvestedService.CompleteFMESpatial(msg);
                }
                Console.WriteLine("New message received : " + msg);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }

    }
}
