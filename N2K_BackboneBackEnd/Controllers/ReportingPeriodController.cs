using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportingPeriodController : ControllerBase
    {
        private readonly IReportingPeriodService _reportingPeriodService;
        private readonly IMapper _mapper;

        public ReportingPeriodController(IReportingPeriodService reportingPeriodService, IMapper mapper)
        {
            _reportingPeriodService = reportingPeriodService;
            _mapper = mapper;
        }

        [Route("Get")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<RepPeriodView>>>> Get()
        {
            var response = new ServiceResponse<List<RepPeriodView>>();
            try
            {
                var repPeriods = await _reportingPeriodService.Get();
                response.Success = true;
                response.Message = "";
                response.Data = repPeriods;
                response.Count = repPeriods.Count();
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<RepPeriodView>();
                return Ok(response);
            }
        }
        
        [Route("Create")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<RepPeriodView>>>> Create(DateTime init, DateTime end)
        {
            var response = new ServiceResponse<List<RepPeriodView>>();
            try
            {
                var repPeriods = await _reportingPeriodService.Create(init, end);
                response.Success = true;
                response.Message = "";
                response.Data = repPeriods;
                response.Count = repPeriods.Count();
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<RepPeriodView>();
                return Ok(response);
            }
        }
        
        [Route("Edit")]
        [HttpPut]
        public async Task<ActionResult<ServiceResponse<List<RepPeriodView>>>> Edit(long id, DateTime newEndDate)
        {
            var response = new ServiceResponse<List<RepPeriodView>>();
            try
            {
                var repPeriods = await _reportingPeriodService.Edit(id, newEndDate);
                response.Success = true;
                response.Message = "";
                response.Data = repPeriods;
                response.Count = repPeriods.Count();
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<RepPeriodView>();
                return Ok(response);
            }
        }
        
        [Route("Close")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<RepPeriodView>>>> Close()
        {
            var response = new ServiceResponse<List<RepPeriodView>>();
            try
            {
                var repPeriods = await _reportingPeriodService.Close();
                response.Success = true;
                response.Message = "";
                response.Data = repPeriods;
                response.Count = repPeriods.Count();
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<RepPeriodView>();
                return Ok(response);
            }
        }
    }
}
