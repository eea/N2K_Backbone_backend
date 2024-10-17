using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

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
        public async Task<ActionResult<FileContentResult>> UnionListDownload(string? bioregs)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                FileContentResult unionListHeader = await _unionListService.UnionListDownload(bioregs ?? "");
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = (bioregs == null) ? 1 : bioregs.Split(',').Length;
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

        [HttpGet("GetUnionListComparer")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> GetUnionListComparer(string? bioregions, int page, int limit)
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
