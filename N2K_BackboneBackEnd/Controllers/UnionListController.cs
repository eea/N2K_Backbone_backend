using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;
using Microsoft.AspNetCore.OData.Query;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class UnionListController : ControllerBase
    {
        private readonly IUnionListService _unionListService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;

        public UnionListController(IUnionListService unionListService, IMapper mapper, IMemoryCache cache)
        {
            _unionListService = unionListService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [Route("Download")]
        [HttpGet]
        public async Task<ActionResult<string>> UnionListDownload(string bioregs)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            try
            {
                string unionListHeader = await _unionListService.UnionListDownload(bioregs);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = bioregs.Split(',').Length;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = String.Empty;
                return Ok(response);
            }
        }

        [HttpGet("GetUnionListComparerSummary")]
        public async Task<ActionResult<ServiceResponse<UnionListComparerSummaryViewModel>>> GetUnionListComparerSummary()
        {
            var response = new ServiceResponse<UnionListComparerSummaryViewModel>();
            try
            {
                var unionListCompareSummary = await _unionListService.GetUnionListComparerSummary(_cache);
                response.Success = true;
                response.Message = "";
                response.Data = unionListCompareSummary;
                response.Count = (unionListCompareSummary == null) ? 0 : unionListCompareSummary.BioRegSiteCodes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new UnionListComparerSummaryViewModel();
                return Ok(response);
            }
        }


        [HttpGet]
        [Route("GetUnionListComparer"),  EnableQuery ]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> GetUnionListComparer([FromQuery(Name = "bioregions")] string? bioregions, [FromQuery(Name = "page")] int page, [FromQuery(Name = "limit")] int limit)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListCompareSummary = await _unionListService.GetUnionListComparer(_cache, bioregions, page, limit);
                response.Success = true;
                response.Message = "";
                response.Data = unionListCompareSummary;
                response.Count = (unionListCompareSummary == null) ? 0 : unionListCompareSummary.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<UnionListComparerDetailedViewModel>();
                return Ok(response);
            }
        }
    }
}
