using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data2;

namespace MyCodeCamp.Controllers
{
	[Produces("application/json")]
    [Route("api/Camps")]
    public class CampsController : Controller
    {
		private readonly ICampRepository _repo;

		public CampsController(ICampRepository campRepository)
		{
			this._repo = campRepository;
		}
			

		[HttpGet("")]
		public IActionResult Get()
		{
			var camps = _repo.GetAllCamps();
			return Ok(camps);
		}
    }
}