using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data2;
using MyCodeCamp.Data2.Entities;
using System;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
	[Produces("application/json")]
	[Route("api/Camps")]
	public class CampsController : Controller
	{
		private readonly ICampRepository _repo;
		private readonly ILogger<CampsController> _logger;

		public CampsController(ICampRepository campRepository, ILogger<CampsController> logger)
		{
			this._repo = campRepository;
			this._logger = logger;
		}


		[HttpGet("")]
		public IActionResult Get()
		{
			var camps = _repo.GetAllCamps();
			return Ok(camps);
		}

		[HttpGet("{id}",Name = "CampGet")]
		public IActionResult Get(int id, bool includeSpeakers = false)
		{ //query string of ?includeSpeakers=true. Additional params are query string
			try
			{
				Camp camp = null;
				if (includeSpeakers)
				{
					camp = _repo.GetCampWithSpeakers(id);
				}
				else
				{
					camp = _repo.GetCamp(id);
				}

				if (camp == null) return NotFound($"Camp {id} was not found");

				return Ok(camp);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]Camp model)
		{
			try
			{
				_logger.LogInformation("Creating a new code camp");
				_repo.Add(model);
				if (await _repo.SaveAllAsync())
				{
					//Produces the URL of the Get with the id of the newly created model.
					var newUri = Url.Link("CampGet", new { id = model.Id });
					return Created(newUri, model);
				}
				else
				{
					_logger.LogWarning("Could not save camp to the database");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error {ex}");
			}
			return BadRequest();
		}
	}
}