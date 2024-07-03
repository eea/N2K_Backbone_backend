using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;
        private readonly IMapper _mapper;

        public DownloadController(IDownloadService downloadService, IMapper mapper)
        {
            _downloadService = downloadService;
            _mapper = mapper;
        }

        [HttpGet("Get/id={id:int}&docuType={docuType:int}")]
        public async Task<ActionResult> DownloadFile(int id, int docuType)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.DownloadFile(id, docuType);
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


        [HttpGet("Get/id={id:int}&docuType={docuType:int}&token={token:string}")]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadFileWithToken(int id, int docuType, string token)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                                
                return await _downloadService.DownloadFile(id, docuType,token);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
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
