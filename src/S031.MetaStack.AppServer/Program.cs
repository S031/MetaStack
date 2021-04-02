using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace S031.MetaStack.AppServer
{
	internal class Program
	{

		/// <summary>
		////Костыль!!! Исключить из ссылок рантайм dll (aka \ORM\*.dll
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static async Task Main(string[] args)
		{
			using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
			{
				await CreateHostBuilder(args)
					.Build()
					// этот токен приходит в BackgroundService.ExecuteAsync
					// не нужно отдельый service
					.RunAsync(cancellationTokenSource.Token);
			}
		}


		private static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(config =>
				{
					config.AddJsonFile("config.json", optional: false, reloadOnChange: true);
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders()
						.AddConsole()
						.AddFile();
				})
				.UseStartup<Startup>()
				.UseConsoleLifetime();
	}
}
