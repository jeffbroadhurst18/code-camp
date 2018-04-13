using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data2;
using MyCodeCamp.Data2.Entities;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
	[Produces("application/json")]
	[Route("api/Camps")]
	public class CampsController : BaseController
	{
		private readonly ICampRepository _repo;
		private readonly ILogger<CampsController> _logger;
		private readonly IMapper _mapper;

		public CampsController(ICampRepository campRepository, ILogger<CampsController> logger, IMapper mapper)
		{
			this._repo = campRepository;
			this._logger = logger;
			this._mapper = mapper;
		}


		[HttpGet("")]
		public IActionResult Get()
		{
			var camps = _repo.GetAllCamps();
			return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
		}

		[HttpGet("{moniker}", Name = "CampGet")]
		public IActionResult Get(string moniker, bool includeSpeakers = false)
		{ //query string of ?includeSpeakers=true. Additional params are query string
			try
			{
				Camp camp = null;
				if (includeSpeakers)
				{
					camp = _repo.GetCampByMonikerWithSpeakers(moniker);
				}
				else
				{
					camp = _repo.GetCampByMoniker(moniker);
				}

				if (camp == null) return NotFound($"Camp {moniker} was not found");

				return Ok(_mapper.Map<CampModel>(camp));
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]CampModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_logger.LogInformation("Creating a new code camp");
				var camp = _mapper.Map<Camp>(model);
				_repo.Add(camp);
				if (await _repo.SaveAllAsync())
				{
					//Produces the URL of the Get with the id of the newly created model.
					var newUri = Url.Link("CampGet", new { moniker = model.Moniker });
					return Created(newUri, _mapper.Map<CampModel>(camp)); //re-do map as camp might have beenchanged by repo
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

		[HttpPut("{moniker}")]//could use patch
		public async Task<IActionResult> Put(string moniker, [FromBody] CampModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var oldCamp = _repo.GetCampByMoniker(moniker);
				if (oldCamp == null)
				{
					return NotFound($"Could not find a camp with moniker {moniker}");
				}

				_mapper.Map<CampModel,Camp>(model, oldCamp); //EXISTING destination object
				//changes the valuesin oldCamp. This is then saved by the SaveAllAsync
				if (await _repo.SaveAllAsync())
				{
					return Ok(_mapper.Map<CampModel>(oldCamp)); //NEW destination object
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error {ex}");
			}
			return BadRequest("Couldn't update Camp");
		}

		[HttpDelete("{moniker}")]
		public async Task<IActionResult> Delete(string moniker)
		{
			try
			{
				var oldCamp = _repo.GetCampByMoniker(moniker);
				if (oldCamp == null)
				{
					return NotFound($"Could not find a camp with moniker {moniker}");
				}
				_repo.Delete(oldCamp);
				if (await _repo.SaveAllAsync())
				{
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error {ex}");
			}
			return BadRequest("Couldn't delete Camp");
		}
	
	}
}