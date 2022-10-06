using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models.backbone_db;
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

        public UnionListController(IUnionListService unionListService, IMapper mapper)
        {
            _unionListService = unionListService;
            _mapper = mapper;
        }

        [Route("UnionList/GetBioRegionTypes")]
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

        [Route("UnionList/GetUnionLists/bioRegion={bioRegionShortCode}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListHeader>>>> GetUnionListHeadersByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<UnionListHeader>>();
            try
            {
                var unionListHeader = await _unionListService.GetUnionListHeadersByBioRegion(bioRegionShortCode);
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

        [Route("UnionList/GetCurrentListDetailed")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListDetail>>>> GetCurrentSitesUnionListDetailByBioRegion(string? bioRegionShortCode)
        {
            var response = new ServiceResponse<List<UnionListDetail>>();
            try
            {
                var unionListDetail = await _unionListService.GetCurrentSitesUnionListDetailByBioRegion(bioRegionShortCode);
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

        [Route("UnionList/GetUnionLists/id={id}")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListHeader>>>> GetUnionListHeadersById(long? id)
        {
            var response = new ServiceResponse<List<UnionListHeader>>();
            try
            {
                var unionListHeader = await _unionListService.GetUnionListHeadersById(id);
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

        [Route("UnionList/Compare")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<UnionListDetail>>>> Compare(long? idTarget, long? idSource)
        {
            var response = new ServiceResponse<List<UnionListDetail>>();
            try
            {
                var unionListDetail = await _unionListService.CompareUnionLists(idTarget, idSource);
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

        [Route("UnionList/Create")]
        [HttpPost]
        public async Task<ActionResult<UnionListHeader>> CreateUnionList(string name, Boolean final)
        {
            ServiceResponse<UnionListHeader> response = new ServiceResponse<UnionListHeader>();
            try
            {
                UnionListHeader unionListHeader = await _unionListService.CreateUnionList(name, final);
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
                response.Data = null;
                return Ok(response);
            }
        }

        [Route("UnionList/Edit")]
        [HttpPut]
        public async Task<ActionResult<UnionListHeader>> EditUnionList(long id, string name, Boolean final)
        {
            ServiceResponse<UnionListHeader> response = new ServiceResponse<UnionListHeader>();
            try
            {
                UnionListHeader unionListHeader = await _unionListService.EditUnionList(id, name, final);
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
                response.Data = null;
                return Ok(response);
            }
        }

        [Route("UnionList/Delete")]
        [HttpDelete]
        public async Task<ActionResult<int>> DeleteUnionList(long id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                int unionListHeader = await _unionListService.DeleteUnionList(id);
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
