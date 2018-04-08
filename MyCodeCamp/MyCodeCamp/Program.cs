using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace MyCodeCamp
{
	public class Program
	{
		//	public static void Main(string[] args)
		//	{
		//		BuildWebHost(args).Run();
		//	}

		//	public static IWebHost BuildWebHost(string[] args) =>
		//		WebHost.CreateDefaultBuilder(args)
		//			.UseStartup<Startup>()
		//			.Build();

		public static void Main(string[] args)
		{
			var logger = NLog.LogManager.LoadConfiguration("nlog.config").GetCurrentClassLogger();
			try
			{
				logger.Debug("Initialising Main");
				BuildWebHost(args).Run();
			}
			catch (Exception e)
			{
				//NLog: catch setup errors
				logger.Error(e, "Stopped program because of exception");
				throw;
			}
		}

		public static IWebHost BuildWebHost(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
					.ConfigureAppConfiguration(SetupConfiguration)
					.UseStartup<Startup>().ConfigureLogging(logging =>
					{
					logging.ClearProviders();
					logging.SetMinimumLevel(LogLevel.Debug);
					}).UseNLog().Build();
		}

		private static void SetupConfiguration(WebHostBuilderContext ctx, IConfigurationBuilder builder)
		{
			//remove the default configuration options
			builder.Sources.Clear();

			builder.AddJsonFile("appsettings.json", false, true);

		}
	}
}
