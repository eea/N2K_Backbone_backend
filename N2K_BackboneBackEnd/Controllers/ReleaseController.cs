using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

using DocumentFormat.OpenXml.ExtendedProperties;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IUnionListService _unionListService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;

        public ReleaseController(IReleaseService releaseService, IUnionListService unionListService, IMapper mapper, IMemoryCache cache)
        {
            _releaseService = releaseService;
            _unionListService = unionListService;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [Route("GetBioRegionTypes")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<BioRegionTypes>>>> GetUnionBioRegionTypes()
        {
            var response = new ServiceResponse<List<BioRegionTypes>>();
            try
            {
                var bioRegionTypes = await _releaseService.GetUnionBioRegionTypes();
                response.Success = true;
                response.Message = "";
                response.Data = bioRegionTypes;
                response.Count = (bioRegionTypes == null) ? 0 : bioRegionTypes.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<BioRegionTypes>();
                return Ok(response);
            }
        }

        [Route("GetReleases")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Releases>>>> GetReleaseHeadersByBioRegion()
        {
            var response = new ServiceResponse<List<Releases>>();
            try
            {
                var unionListHeader = await _releaseService.GetReleaseHeadersByBioRegion(null);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = (unionListHeader == null) ? 0 : unionListHeader.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Releases>();
                return Ok(response);
            }
        }

        [Route("GetReleases/bioRegion={bioRegionShortCode}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Releases>>>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<Releases>>();
            try
            {
                var unionListHeader = await _releaseService.GetReleaseHeadersByBioRegion(bioRegionShortCode);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = (unionListHeader == null) ? 0 : unionListHeader.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Releases>();
                return Ok(response);
            }
        }

        [Route("GetReleases/id={id}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Releases>>>> GetReleaseHeadersById(long? id)
        {
            var response = new ServiceResponse<List<Releases>>();
            try
            {
                var unionListHeader = await _releaseService.GetReleaseHeadersById(id);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = (unionListHeader == null) ? 0 : unionListHeader.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<Releases>();
                return Ok(response);
            }
        }

        [Route("GetCurrentListDetailed")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<ReleaseDetail>>>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<ReleaseDetail>>();
            try
            {
                var unionListDetail = await _releaseService.GetCurrentSitesReleaseDetailByBioRegion(bioRegionShortCode);
                response.Success = true;
                response.Message = "";
                response.Data = unionListDetail;
                response.Count = (unionListDetail == null) ? 0 : unionListDetail.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new List<ReleaseDetail>();
                return Ok(response);
            }
        }


        [HttpGet("GetCompareSummary/idSource={idSource:int}&idTarget={idTarget:int}")]
        public async Task<ActionResult<ServiceResponse<UnionListComparerSummaryViewModel>>> GetCompareSummary(long? idSource, long? idTarget)
        {
            var response = new ServiceResponse<UnionListComparerSummaryViewModel>();
            try
            {
                var unionListCompareSummary= await _unionListService.GetCompareSummary(idSource, idTarget, null, _cache);
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
                response.Data = new UnionListComparerSummaryViewModel() ;
                return Ok(response);
            }
        }

        [HttpGet("GetCompareSummary/idSource={idSource:int}&idTarget={idTarget:int}&bioRegions={bioRegions:string}")]
        public async Task<ActionResult<ServiceResponse<UnionListComparerSummaryViewModel>>> GetCompareSummaryByBioRegion(long? idSource, long? idTarget, string? bioRegions )
        {
            var response = new ServiceResponse<UnionListComparerSummaryViewModel>();
            try
            {
                var unionListCompareSummary = await _unionListService.GetCompareSummary(idSource, idTarget, bioRegions,_cache);
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

        [HttpGet("Compare")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> CompareByBioRegion([FromQuery(Name = "idSource")] long idSource, [FromQuery(Name = "idTarget")] long idTarget, [FromQuery(Name = "bioregions")] string? bioregions)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget, bioregions, _cache);
                response.Success = true;
                response.Message = "";
                response.Data = unionListDetail;
                response.Count = (unionListDetail == null) ? 0 : unionListDetail.Count;
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

        [HttpGet("ComparePaginated")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> ComparePaginatedByBioRegion([FromQuery(Name = "idSource")] long idSource, [FromQuery(Name = "idTarget")] long idTarget, [FromQuery(Name = "bioregions")] string? bioregions, [FromQuery(Name = "page")] int page, [FromQuery(Name = "limit")] int limit)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget, bioregions, _cache, page, limit);
                response.Success = true;
                response.Message = "";
                response.Data = unionListDetail;
                response.Count = (unionListDetail == null) ? 0 : unionListDetail.Count;
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

        [Route("Create")]
        [HttpPost]
        public async Task<ActionResult<List<Releases>>> CreateRelease([FromBody] ReleasesInputParam release)
        {
            ServiceResponse<List<Releases>> response = new ServiceResponse<List<Releases>>();
            try
            {
                List<Releases> unionListHeader = await _releaseService.CreateRelease(release.Name, release.Final, release.Character, release.Comments);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = unionListHeader == null ? 0 : unionListHeader.Count;
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

        [Route("Update")]
        [HttpPut]
        public async Task<ActionResult<List<Releases>>> UpdateRelease([FromBody] UnionListHeaderInputParam unionList)
        {
            ServiceResponse<List<Releases>> response = new ServiceResponse<List<Releases>>();
            try
            {
                List<Releases> unionListHeader = await _releaseService.UpdateRelease(unionList.Id, unionList.Name, unionList.Final.HasValue ? unionList.Final.Value : false);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
                response.Count = (unionListHeader == null) ? 0 : unionListHeader.Count;
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

        [Route("Delete")]
        [HttpDelete]
        public async Task<ActionResult<int>> DeleteRelease([FromBody] long id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                int unionListHeader = await _releaseService.DeleteRelease(id);
                response.Success = true;
                response.Message = "";
                response.Data = unionListHeader;
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
