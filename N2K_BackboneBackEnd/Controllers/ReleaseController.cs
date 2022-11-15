using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;
using System.Web.Http.OData;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly IUnionListService _unionListService;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;

        public ReleaseController(IUnionListService unionListService, IReleaseService releaseService, IMapper mapper, IMemoryCache cache)
        {
            _unionListService = unionListService;
            _releaseService = releaseService;
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
                var bioRegionTypes = await _unionListService.GetUnionBioRegionTypes();
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
        public async Task<ActionResult<ServiceResponse<List<UnionListHeader>>>> GetReleaseHeadersByBioRegion()
        {
            var response = new ServiceResponse<List<UnionListHeader>>();
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
                response.Data = new List<UnionListHeader>();
                return Ok(response);
            }
        }

        [Route("GetReleases/bioRegion={bioRegionShortCode}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListHeader>>>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<UnionListHeader>>();
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
                response.Data = new List<UnionListHeader>();
                return Ok(response);
            }
        }

        [Route("GetReleases/id={id}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListHeader>>>> GetReleaseHeadersById(long? id)
        {
            var response = new ServiceResponse<List<UnionListHeader>>();
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
                response.Data = new List<UnionListHeader>();
                return Ok(response);
            }
        }

        [Route("GetCurrentListDetailed")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListDetail>>>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<UnionListDetail>>();
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
                response.Data = new List<UnionListDetail>();
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


        [HttpGet("Compare/idSource={idSource:int}&idTarget={idTarget:int}")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> Compare(long idSource, long idTarget)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget,null,_cache);
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

        [HttpGet("Compare/idSource={idSource:int}&idTarget={idTarget:int}&bioRegions={bioRegions:string}")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> CompareByBioRegion(long idSource, long idTarget,string bioRegions)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget, bioRegions, _cache);
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


        [HttpGet("Compare/idSource={idSource:int}&idTarget={idTarget:int}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> ComparePaginated(long idSource, long idTarget, int page, int limit)
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget,null,_cache, page,limit);
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

        [HttpGet("Compare/idSource={idSource:int}&idTarget={idTarget:int}&bioRegions={bioRegions:string}&page={page:int}&limit={limit:int}")]
        public async Task<ActionResult<ServiceResponse<List<UnionListComparerDetailedViewModel>>>> ComparePaginatedByBioregion(long idSource, long idTarget,string bioRegions, int page, int limit)    
        {
            var response = new ServiceResponse<List<UnionListComparerDetailedViewModel>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idSource, idTarget, bioRegions,_cache, page, limit);
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
        public async Task<ActionResult<List<UnionListHeader>>> CreateRelease([FromBody] UnionListHeaderInputParam unionList)
        {
            ServiceResponse<List<UnionListHeader>> response = new ServiceResponse<List<UnionListHeader>>();
            try
            {
                List<UnionListHeader> unionListHeader = await _releaseService.CreateRelease(unionList.Name, unionList.Final.HasValue ? unionList.Final.Value : false);
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
        public async Task<ActionResult<List<UnionListHeader>>> UpdateRelease([FromBody] UnionListHeaderInputParam unionList)
        {
            ServiceResponse<List<UnionListHeader>> response = new ServiceResponse<List<UnionListHeader>>();
            try
            {
                List<UnionListHeader> unionListHeader = await _releaseService.UpdateRelease(unionList.Id, unionList.Name, unionList.Final.HasValue ? unionList.Final.Value : false);
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
                response.Count = (unionListHeader == null) ? 0 : 1;
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
