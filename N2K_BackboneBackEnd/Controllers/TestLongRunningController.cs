using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class TestLongRunningController : ControllerBase
    {

        private readonly IHarvestedService _harvestedService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;
        private readonly BackgroundWorkerQueue _backgroundWorkerQueue;

        public TestLongRunningController(IHarvestedService harvestedService, IMapper mapper, IMemoryCache cache, BackgroundWorkerQueue backgroundWorkerQueue)
        {
            _harvestedService = harvestedService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _backgroundWorkerQueue = backgroundWorkerQueue;
        }


        [Route("HarvestSpatial")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<int>>> HarvestSpatialBackground([FromBody] EnvelopesToProcess[] envelopes)
        {
            var response = new ServiceResponse<int>();
            try
            {
                await Task.Delay(1);
                _backgroundWorkerQueue.QueueBackgroundWorkItem( async token =>
                {
                    //await _harvestedService.FullHarvest(_cache);
                    await _harvestedService.HarvestSpatialData(envelopes);
                    var a = 1;
                });
                
                var resp = 1; // await _longRunningService.TestLongRun();
                response.Success = true;
                response.Message = "";
                response.Data = resp;
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

    }
}
