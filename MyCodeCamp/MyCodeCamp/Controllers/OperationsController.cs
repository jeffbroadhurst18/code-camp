using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
		private readonly ILogger<OperationsController> _logger;
		private readonly IConfiguration _config;

		public OperationsController(ILogger<OperationsController> logger, IConfiguration config)
		{
			this._logger = logger;
			this._config = config;
		}

		[HttpOptions("reloadConfig")]
		public IActionResult ReloadConfiguration()
		{
			try
			{
				((IConfigurationRoot)_config).Reload();
				return Ok("Configuration reloaded");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception was thrown when reloading configuration: {ex}");
			}
			return BadRequest();
		}
    }
}