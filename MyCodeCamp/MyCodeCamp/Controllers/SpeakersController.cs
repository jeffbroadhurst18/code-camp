using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data2;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/camps/{moniker}/speakers")]
    public class SpeakersController : BaseController
    {
		private readonly ICampRepository _repo;
		private readonly ILogger<SpeakersController> _logger;
		private readonly IMapper _mapper;

		public SpeakersController(ICampRepository repo, ILogger<SpeakersController> logger, IMapper mapper)
		{
			this._repo = repo;
			this._logger = logger;
			this._mapper = mapper;
		}

		[HttpGet]
		public IActionResult Get(string moniker)
		{
			var speakers = _repo.GetSpeakersByMoniker(moniker);
			return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
		}

		[HttpGet("{id}", Name = "SpeakerGet")]
		public IActionResult Get(string moniker, int id)
		{
			var speaker = _repo.GetSpeaker(id);
			if (speaker == null) {
				return NotFound();
			}
			if (speaker.Camp.Moniker != moniker)
			{
				return BadRequest("Speaker is not in the specified camp");
			}

			return Ok(_mapper.Map<SpeakerModel>(speaker));
		}

	}
}