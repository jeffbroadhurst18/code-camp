using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data2;
using MyCodeCamp.Data2.Entities;

namespace MyCodeCamp
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			_config = configuration;
			_env = env;
		}

		private IConfiguration _config;
		private readonly IHostingEnvironment _env;

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//Scoped for use by instances in scope of currnt request
			//Transient - recreated everywhere it is used
			//Singleton - one instance shared by everything during lifetime of web server
			services.AddSingleton(_config);
			services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
			services.AddScoped<ICampRepository, CampRepository>(); //scope = single request
			services.AddTransient<CampDbInitializer>();
			services.AddTransient<CampIdentityInitializer>();
			services.AddAutoMapper(); //Adds IMapper as injectable type
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddIdentity<CampUser, IdentityRole>()
				.AddEntityFrameworkStores<CampContext>();//.AddDefaultTokenProviders();

			services.AddApiVersioning(cfg => {
				cfg.DefaultApiVersion = new ApiVersion(1, 1);
				cfg.AssumeDefaultVersionWhenUnspecified = true;
				cfg.ReportApiVersions = true;
			});

			//services.ConfigureApplicationCookie(cfg =>
			//{
			//	cfg.Events = new CookieAuthenticationEvents()
			//	{
			//		OnRedirectToLogin = (ctx) =>
			//		{
			//			if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
			//			{
			//				//redirec to login produces the 200
			//				//Here we don't redirect bu just return the 401 UNAUTHORIZED
			//				ctx.Response.StatusCode = 401;
			//			}
			//			return Task.CompletedTask;
			//		},
			//		OnRedirectToAccessDenied = (ctx) =>
			//		{
			//			if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
			//			{
			//				//redirect to access denied produces the 200
			//				//Here we don't redirect bu just return the 403 FORBIDDEN
			//				ctx.Response.StatusCode = 403;
			//			}
			//			return Task.CompletedTask;
			//		}
			//	};
			//});


			//services.AddCors(cfg => {
			//	cfg.AddPolicy("Wildermuth", bldr =>
			//	{
			//		bldr.AllowAnyHeader().
			//		AllowAnyMethod().
			//		AllowAnyOrigin();
			//	//	WithOrigins("http://wildermuth.com");
			//	}); //this defines the policy but doesn't implement it.
			//	cfg.AddPolicy("AnyGET", bldr =>
			//	{
			//		bldr.AllowAnyHeader().
			//		WithMethods("GET").
			//		AllowAnyOrigin();
			//	});
			//});

			services.AddAuthorization(cfg =>
			{
				cfg.AddPolicy("SuperUsers", p => p.RequireClaim("SuperUser", "True"));
			}); //Creates the Policy for authorization

			services.AddAuthentication().AddJwtBearer(cfg =>
			{
				cfg.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidIssuer = _config["Tokens:Issuer"],
					ValidAudience = _config["Tokens:Audience"],
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"])),
					ValidateLifetime = true
				};
			});

			//Filters are a way of interrupting calls to individual actions.
			//Ones defined here are applicable across all actions in project
			services.AddMvc(opt =>
			{
				if (!IsProduction())
				{
					opt.SslPort = 44388;
				}
				opt.Filters.Add(new RequireHttpsAttribute()); //Require HTTPS. changes Http requests to Https
			}).AddJsonOptions(opt =>
			{
				opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env,
			CampDbInitializer seeder, ILoggerFactory loggerFactory, CampIdentityInitializer identitySeeder)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			loggerFactory.AddConsole(_config.GetSection("Logging"));
			loggerFactory.AddDebug();

			//identity before mvc

			app.UseAuthentication();

			app.UseMvc();
			seeder.Seed().Wait(); //async
			identitySeeder.Seed().Wait();


		}

		private bool IsProduction()
		{
			return _env.IsProduction();
		}
	}
}
