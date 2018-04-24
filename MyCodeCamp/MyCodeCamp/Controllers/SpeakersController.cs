using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data2;
using MyCodeCamp.Data2.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
	[Produces("application/json")]
    [Route("api/camps/{moniker}/speakers")]
	[ValidateModel]
	[ApiVersion("1.0")]
	[ApiVersion("1.1")]
	public class SpeakersController : BaseController
    {
		protected readonly ICampRepository _repo;
		protected readonly ILogger<SpeakersController> _logger;
		protected readonly IMapper _mapper;
		protected readonly UserManager<CampUser> _userMgr;

		public SpeakersController(ICampRepository repo, ILogger<SpeakersController> logger, IMapper mapper,
			UserManager<CampUser> userMgr)
		{
			this._repo = repo;
			this._logger = logger;
			this._mapper = mapper;
			this._userMgr = userMgr;
		}

		[HttpGet]
		[MapToApiVersion("1.0")]
		public IActionResult Get(string moniker,bool includeTalks = false)
		{
			var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);
			return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
		}

		[HttpGet]
		[MapToApiVersion("1.1")]
		public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
		{
			var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);
			return Ok(new { count = speakers.Count(), results = _mapper.Map<IEnumerable<SpeakerModel>>(speakers) });
		}  

		[HttpGet("{id}", Name = "SpeakerGet")]
		public IActionResult Get(string moniker, int id, bool includeTalks = false)
		{
			var speaker = includeTalks ? _repo.GetSpeakerWithTalks(id) : _repo.GetSpeaker(id);
			if (speaker == null) {
				return NotFound();
			}
			if (speaker.Camp.Moniker != moniker)
			{
				return BadRequest("Speaker is not in the specified camp");
			}

			return Ok(_mapper.Map<SpeakerModel>(speaker));
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Post(string moniker, [FromBody]SpeakerModel model)
		{
			try
			{
				// We are adding the speaker to the camp.
				var camp = _repo.GetCampByMoniker(moniker);
				if (camp == null)
				{
					return BadRequest("Could not find camp");
				}

				var speaker = _mapper.Map<Speaker>(model);
				speaker.Camp = camp;

				var campUser = await _userMgr.FindByNameAsync(this.User.Identity.Name);
				if (campUser != null)
				{
					speaker.User = campUser;

					_repo.Add(speaker);
					if (await _repo.SaveAllAsync())
					{
						var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
						return Created(url, _mapper.Map<SpeakerModel>(speaker));
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown while adding speaker: {ex}");
			}
			return BadRequest("Could not add new speaker");
		}


		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Put(string moniker, int id,[FromBody]SpeakerModel model)
		{
			try
			{
				var speaker = _repo.GetSpeaker(id);
				if (speaker == null)
				{
					return NotFound("Can't find a speaker with that id");
				}
				
				if (speaker.Camp.Moniker != moniker)
				{
					return BadRequest("Speaker isn't part of that camp");
				}

				if (speaker.User.UserName == this.User.Identity.Name) //Loggedin User
				{
					return Forbid(); //You can't amend speaker to anyone other than yourself.
				}

				_mapper.Map(model, speaker);//map from model to speaker
				// speaker is updated so differences will now be saved
				if (await _repo.SaveAllAsync())
				{
					return Ok(_mapper.Map<SpeakerModel>(speaker));
				}

			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown while updating speaker: {ex}");
			}
			return BadRequest("Could not update speaker");
		}

		[HttpDelete("{id}")]
		[Authorize] //could have allow anonymous to not require authroisation
		public async Task<IActionResult> Delete(string moniker, int id)
		{
			try
			{
				var speaker = _repo.GetSpeaker(id);
				if (speaker == null)
				{
					return NotFound("Can't find a speaker with that id");
				}

				if (speaker.Camp.Moniker != moniker)
				{
					return BadRequest("Speaker isn't part of that camp");
				}

				if (speaker.User.UserName == this.User.Identity.Name) //Loggedin User
				{
					return Forbid(); //You can't amend speaker to anyone other than yourself.
				}

				_repo.Delete(speaker);
				//Deletes the speaker record that will take care the speaker value in other entities (or you have to build the rules)
				if (await _repo.SaveAllAsync())
				{
					return Ok();
				}

			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown while deleting speaker: {ex}");
			}
			return BadRequest("Could not delete speaker");
		}
	}
}