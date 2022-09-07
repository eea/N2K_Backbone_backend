using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.ServiceResponse;
using N2K_BackboneBackEnd.Services;
using Microsoft.AspNetCore.Authorization;

namespace N2K_BackboneBackEnd.Controllers
{
    [Authorize(AuthenticationSchemes = "EULoginSchema")]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {

        private readonly IConfigService _configService;
        private readonly IMapper _mapper;

        public ConfigurationController(IConfigService configService, IMapper mapper)
        {
            _configService = configService;
            _mapper = mapper;
        }


        [Route("Get")]
        [HttpGet]
        public async Task<ActionResult<String>> Get()
        {
            var response = new ServiceResponse<string>();
            try
            {
                var config = await _configService.GetFrontEndConfiguration();
                response.Success = true;
                response.Message = "";
                response.Data = config;
                response.Count = (config == null) ? 0 :1;
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

