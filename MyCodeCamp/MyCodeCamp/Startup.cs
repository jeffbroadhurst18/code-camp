using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCodeCamp.Data2;

namespace MyCodeCamp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

		private IConfiguration _config;

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
			services.AddMvc().AddJsonOptions(opt => {
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
    }
}
