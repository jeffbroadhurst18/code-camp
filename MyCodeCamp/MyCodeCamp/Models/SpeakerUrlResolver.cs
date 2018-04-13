using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
	public class SpeakerUrlResolver : IValueResolver<Speaker, SpeakerModel, string>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public SpeakerUrlResolver(IHttpContextAccessor httpContextAccessor)//parameter passed by DI
		{
			this._httpContextAccessor = httpContextAccessor;
		}

		public string Resolve(Speaker source, SpeakerModel destination, string destMember, ResolutionContext context)
		{
			var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
			return url.Link("SpeakerGet", new { moniker = source.Camp.Moniker, id = source.Id });
		}
	}
}
