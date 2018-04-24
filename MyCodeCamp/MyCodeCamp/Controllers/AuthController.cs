using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data2;
using MyCodeCamp.Data2.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/Auth")]
    public class AuthController : Controller
    {
		private readonly CampContext _context;
		private readonly SignInManager<CampUser> _signInMgr;
		private readonly ILogger<AuthController> _logger;
		private readonly UserManager<CampUser> _userManager;
		private readonly IPasswordHasher<CampUser> _hasher;
		private readonly IConfiguration _config;
		
		public AuthController(CampContext context, SignInManager<CampUser> signInMgr, 
			ILogger<AuthController> logger, UserManager<CampUser> userManager, IPasswordHasher<CampUser> hasher,
			IConfiguration config )
		{
			this._context = context;
			this._signInMgr = signInMgr;
			this._logger = logger;
			this._userManager = userManager;
			this._hasher = hasher;
			this._config = config;
		}

		[HttpPost("login")]
		[ValidateModel]
		[EnableCors("Wildermuth")]
		public async Task<IActionResult> Login([FromBody] CredentialModel model)
		{
			try
			{
				var result = await _signInMgr.PasswordSignInAsync(model.UserName, model.Password, false, false);
				// first false means cookie isn't persisted after browse is closed
				if (result.Succeeded)
				{
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown while logging in: {ex}");
			}
			return BadRequest("Failed to login");
		}

		[ValidateModel]
		[HttpPost("token")]
		public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
		{
			try
			{
				var user = await _userManager.FindByNameAsync(model.UserName);
				if (user != null)
				{
					//verifies if the provided password is the same as the one in the database.
					if (_hasher.VerifyHashedPassword(user,user.PasswordHash,model.Password) == PasswordVerificationResult.Success)
					{
						var userClaims = await _userManager.GetClaimsAsync(user); //claims from identity system eg.roles

						var claims = new[]
						{
							new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
							//Jti is data that ensures uniqueness i.e. guid
							new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
							new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
							new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
							new Claim(JwtRegisteredClaimNames.Email, user.Email)
						}.Union(userClaims); // use Union to combine two arrays

						//The string should be stored in config
						var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
						var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

						var token = new JwtSecurityToken(
							issuer: _config["Tokens:Issuer"],
							audience: _config["Tokens:Audience"], 
							claims: claims,
							expires: DateTime.UtcNow.AddMinutes(20),
							signingCredentials: creds);

						return Ok(new
						{
							token = new JwtSecurityTokenHandler().WriteToken(token),
							expiration = token.ValidTo
						});
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown while creating JWT - {ex}");
			}
			return BadRequest("Could not create the token");
		}

    }
}