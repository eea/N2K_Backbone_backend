using AutoMapper;
using Microsoft.AspNetCore.Http;
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

        /*
        public EULoginController( IEULoginService euLoginService, IMapper mapper)
        {
            _euLoginService = euLoginService;
            _mapper = mapper;
        }
        */


        public EULoginController(IOptions<ConfigSettings> app, IEULoginService euLoginService, IMapper mapper )
        {
            _appSettings = app;
            _euLoginService = euLoginService;
            _mapper= mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<string>>> GetLoginUrl (string redirectionUrl)
        {
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
    }
}
