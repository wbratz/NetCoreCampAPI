using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OperationsController(IConfiguration configuration )
        {
            _configuration = configuration;
        }

        [HttpOptions("reload-config")]
        public IActionResult ReloadConfig()
        {
            try
            {
                var root = (IConfigurationRoot)_configuration;
                root.Reload();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        
    }
}
