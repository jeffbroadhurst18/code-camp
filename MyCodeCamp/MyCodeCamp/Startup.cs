using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCodeCamp.Data2;

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
			services.AddAutoMapper(); //Adds IMapper as injectable type
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddCors(cfg => {
				cfg.AddPolicy("Wildermuth", bldr =>
				{
					bldr.AllowAnyHeader().
					AllowAnyMethod().
					WithOrigins("http://wildermuth.com");
				}); //this defines the policy but doesn't implement it.
				cfg.AddPolicy("AnyGET", bldr =>
				{
					bldr.AllowAnyHeader().
					WithMethods("GET").
					AllowAnyOrigin();
				});
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
			CampDbInitializer seeder, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			loggerFactory.AddConsole(_config.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseMvc();
			seeder.Seed().Wait(); //async


		}

		private bool IsProduction()
		{
			return _env.IsProduction();
		}
	}
}
