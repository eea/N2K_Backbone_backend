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
    public class ExtractionController : ControllerBase
    {
        private readonly IExtractionService _extractionService;
		private readonly IDownloadService _downloadService;
        private readonly IMapper _mapper;

        public ExtractionController(IExtractionService extractionService, IDownloadService downloadService, IMapper mapper)
        {
            _extractionService = extractionService;
			_downloadService = downloadService;
            _mapper = mapper;
        }

        [Route("Update")]
		[HttpPost]
        public async Task<ActionResult> UpdateExtractions()
        {
            ServiceResponse<ActionResult> response = new();
            try
            {
                await _extractionService.UpdateExtraction();
                response.Success = true;
                response.Message = "";
                response.Count = 0;
                response.Data = null;
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

		[HttpGet("GetLast")]
		public async Task<string> GetLast()
		{
			ServiceResponse<string> response = new();
			try
			{
				 return _extractionService.GetLast();
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

		[HttpGet("Download")]
		public async Task<ActionResult> DownloadExtractions()
		{
			ServiceResponse<FileContentResult> response = new();
			try
			{
				return await _downloadService.DownloadExtractionsFile();
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
