using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using TaskPlus.Server.Logging.File;

namespace TaskPlus.Server
{
	public class Program
	{
		public static void Main(string[] args)
			=> CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			//var logSettings = configuration.GetSection("ApplicationLogSettings")?.Get<FileLogSettings>() ?? FileLogSettings.Default;

			return Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(config =>
				{
					config.AddConfiguration(configuration);
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
					logging.AddFileLogger();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
		}
	}
}
