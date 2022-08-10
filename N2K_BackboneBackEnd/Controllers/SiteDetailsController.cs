using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.ServiceResponse;
using AutoMapper;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteDetailsController : ControllerBase
    {

        private readonly ISiteDetailsService _siteDetailsService;
        private readonly IMapper _mapper;


        public SiteDetailsController(ISiteDetailsService siteDetailsService, IMapper mapper)
        {
            _siteDetailsService = siteDetailsService;
            _mapper = mapper;
        }


        #region SiteGeometry
        [HttpGet("GetSiteGeometry/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<SiteGeometryDetailed>> GetSiteGeometry(string pSiteCode, int pCountryVersion)
        {
            ServiceResponse<SiteGeometryDetailed> response = new ServiceResponse<SiteGeometryDetailed>();
            try
            {
                SiteGeometryDetailed siteGeometry = await _siteDetailsService.GetSiteGeometry(pSiteCode, pCountryVersion);
                response.Success = true;
                response.Message = "";
                response.Data = siteGeometry;
                response.Count = (siteGeometry == null) ? 0 : 1;
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


        #endregion


        #region SiteComments
        [HttpGet("GetSiteComments/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<List<StatusChanges>>> ListSiteComments(string pSiteCode, int pCountryVersion)
        {
            ServiceResponse<List<StatusChanges>> response = new ServiceResponse<List<StatusChanges>>();
            try
            {
                List<StatusChanges> siteComments = await _siteDetailsService.ListSiteComments(pSiteCode, pCountryVersion);
                response.Success = true;
                response.Message = "";
                response.Data = siteComments;
                response.Count = (siteComments == null) ? 0 : siteComments.Count;
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


        [Route("SiteComments/Add")]
        [HttpPost]
        public async Task<ActionResult<List<StatusChanges>>> AddComment([FromBody] StatusChanges comment)
        {
            ServiceResponse<List<StatusChanges>> response = new ServiceResponse<List<StatusChanges>>();
            try
            {
                List<StatusChanges> siteComments = await _siteDetailsService.AddComment(comment);
                response.Success = true;
                response.Message = "";
                response.Data = siteComments;
                response.Count = (siteComments == null) ? 0 : siteComments.Count;
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


        [Route("SiteComments/Delete")]
        [HttpDelete]
        public async Task<ActionResult<int>> DeleteComment([FromBody] int Id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                int siteComments = await _siteDetailsService.DeleteComment(Id);
                response.Success = true;
                response.Message = "";
                response.Data = siteComments;
                response.Count = (siteComments == null) ? 0 : 1;
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


        [Route("SiteComments/Update")]
        [HttpPut]
        public async Task<ActionResult<List<StatusChanges>>> UpdateComment([FromBody] StatusChanges comment)
        {
            ServiceResponse<List<StatusChanges>> response = new ServiceResponse<List<StatusChanges>>();
            try
            {
                List<StatusChanges> siteComments = await _siteDetailsService.UpdateComment(comment);
                response.Success = true;
                response.Message = "";
                response.Data = siteComments;
                response.Count = (siteComments == null) ? 0 : siteComments.Count;
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
        #endregion


        #region SiteFiles
        [HttpGet("GetAttachedFiles/siteCode={pSiteCode}&version={pCountryVersion}")]
        public async Task<ActionResult<List<JustificationFiles>>> ListSiteFiles(string pSiteCode, int pCountryVersion)
        {
            var response = new ServiceResponse<List<JustificationFiles>>();
            try
            {
                List<JustificationFiles> siteFiles = await _siteDetailsService.ListSiteFiles(pSiteCode, pCountryVersion);
                response.Success = true;
                response.Message = "";
                response.Data = siteFiles;
                response.Count = (siteFiles == null) ? 0 : siteFiles.Count;
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


        [Route("UploadAttachedFile")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<List<JustificationFiles>>> UploadFile([FromQuery] AttachedFile attachedFiles)
        {
            var response = new ServiceResponse<List<JustificationFiles>>();
            try
            {
                if (attachedFiles.Files == null || attachedFiles.Files.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No file selected";
                    response.Count = 0;
                    response.Data = null;
                    return Ok(response);
                }
                //var formCollection = await Request.ReadFormAsync();
                List<JustificationFiles> siteFiles = await _siteDetailsService.UploadFile(attachedFiles);
                response.Success = true;
                response.Message = "";
                response.Data = siteFiles;
                response.Count = (siteFiles == null) ? 0 : siteFiles.Count;
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

        [Route("DeleteAttachedFile")]
        [HttpDelete]
        public async Task<ActionResult<int>> DeleteFile([FromQuery] long justificationId)
        {

            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                int siteComments = await _siteDetailsService.DeleteFile(justificationId);
                response.Success = true;
                response.Message = "";
                response.Data = siteComments;
                response.Count = (siteComments == null) ? 0 : 1;
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

        #endregion


        [Route("SaveEdition/")]
        [HttpPost]
        public async Task<ActionResult<string>> SaveEdition([FromBody] ChangeEditionDb changeEdition)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var siteChanges = await _siteDetailsService.SaveEdition(changeEdition);
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
                response.Data = null;
                return Ok(response);
            }
        }

        [Route("GetReferenceEditInfo/")]
        [HttpGet]
        public async Task<ActionResult<ChangeEditionViewModel>> GetReferenceEditInfo(string siteCode)
        {
            var response = new ServiceResponse<ChangeEditionViewModel>();
            try
            {
                var siteChange = await _siteDetailsService.GetReferenceEditInfo(siteCode);
                response.Success = true;
                response.Message = "";
                response.Data = siteChange;
                response.Count = siteChange == null ? 1 : 0;
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
    }
}
