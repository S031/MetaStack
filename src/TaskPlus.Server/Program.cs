using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskPlus.Server
{
	public class Program
	{
		public static void Main(string[] args)
			=> CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(config =>
				{
					config.AddJsonFile("config.json", optional: false, reloadOnChange: true);
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
					logging.AddFile();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
