using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;


namespace N2K_BackboneBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EULoginController : ControllerBase
    {
        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly IEULoginService _euLoginService;
        private readonly IMapper _mapper;



        public EULoginController(IOptions<ConfigSettings> app, IEULoginService euLoginService, IMapper mapper )
        {
            _appSettings = app;
            _euLoginService = euLoginService;
            _mapper= mapper;
        }

        [HttpGet("GetLoginUrl/redirectionUrl={redirectionUrl}")]
        public async Task<ActionResult<ServiceResponse<string>>> GetLoginUrl (string redirectionUrl)
        {
            redirectionUrl = Uri.UnescapeDataString(redirectionUrl);
            var response = new ServiceResponse<string>();
            try
            {              
                var url = await _euLoginService.GetLoginUrl(redirectionUrl);
                response.Success = true;
                response.Message = "";
                response.Data = url;
                response.Count = (url == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = "";
                return Ok(response);
            }

        }

        /// <summary>
        /// This method is awesome and solves your problems
        /// </summary>
        /// <param name="redirectionUrl">The url to be redirected to</param>
        /// <param name="code_challenge">A code challenge generated in javascript via base64URL(CryptoJS.SHA256(code_verifier)))</param>
        [HttpGet("GetLoginUrlByCodeChallenge/redirectionUrl={redirectionUrl}&code_challenge={code_challenge}")]

        public async Task<ActionResult<ServiceResponse<string>>> GetLoginUrl(string redirectionUrl,string code_challenge)
        {
            redirectionUrl = Uri.UnescapeDataString(redirectionUrl);
            var response = new ServiceResponse<string>();
            try
            {
                var url = await _euLoginService.GetLoginUrl(redirectionUrl, code_challenge);
                response.Success = true;
                response.Message = "";
                response.Data = url;
                response.Count = (url == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = "";
                return Ok(response);
            }

        }


        [HttpGet("GetToken/redirectionUrl={redirectionUrl}&code={code}&code_verifier={code_verifier}")]
        public async Task<ActionResult<ServiceResponse<string>>> GetToken(string redirectionUrl, string code, string code_verifier)
        {            
            var response = new ServiceResponse<string>();
            redirectionUrl = Uri.UnescapeDataString(redirectionUrl);
            try
            {
                var url = await _euLoginService.GetToken(redirectionUrl, code, code_verifier);
                response.Success = true;
                response.Message = "";
                response.Data = url;
                response.Count = (url == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = "";
                return Ok(response);
            }

        }


        [HttpGet("GetUsername/token={token}")]
        public async Task<ActionResult<ServiceResponse<string>>> GetUsername(string token)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var url = await _euLoginService.GetUsername(token);
                response.Success = true;
                response.Message = "";
                response.Data = url;
                response.Count = (url == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = "";
                return Ok(response);
            }

        }


    }
}
