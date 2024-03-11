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
    public class DownloadController : Controller
    {
        private readonly IDownloadService _downloadService;
        private readonly IMapper _mapper;

        public DownloadController(IDownloadService downloadService, IMapper mapper)
        {
            _downloadService = downloadService;
            _mapper = mapper;
        }

        [HttpGet("/justificationfiles/{filename:String}")]
        public async Task<ActionResult> DownloadFile(string filename)
        {
            var response = new ServiceResponse<string>();
            try
            {
                return await _downloadService.DownloadFile(filename);
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

        [HttpGet("/justificationfiles/{filename:String}/alias={alias:string}")]
        public async Task<ActionResult> DownloadFileAsAlias(string filename, string alias)
        {
            var response = new ServiceResponse<string>();
            try
            {
                return await _downloadService.DownloadAsFilename(filename, alias);
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


        [HttpGet("/justificationfiles/{filename:string}&token={token:string}")]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadFileWithToken(string filename, string token)
        {
            var response = new ServiceResponse<string>();
            try
            {
                return await _downloadService.DownloadFile(filename, token);
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

        [HttpGet("/justificationfiles/{filename:string}/alias={alias:string}&token={token:string}")]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadFileAliasWithToken(string filename, string alias, string token)
        {
            var response = new ServiceResponse<string>();
            try
            {
                return await _downloadService.DownloadAsFilename(filename, alias, token);
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