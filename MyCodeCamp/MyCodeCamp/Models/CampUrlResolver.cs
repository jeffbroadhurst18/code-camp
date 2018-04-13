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
	public class CampUrlResolver : IValueResolver<Camp, CampModel, string>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CampUrlResolver(IHttpContextAccessor httpContextAccessor)//parameter passed by DI
		{
			this._httpContextAccessor = httpContextAccessor;
		}

		public string Resolve(Camp source, CampModel destination, string destMember, ResolutionContext ctx)
		{
			var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
			return url.Link("CampGet", new { moniker = source.Moniker });
		}
	}
}
