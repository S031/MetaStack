using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
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
				var t = cancellationTokenSource.Token;
				await CreateHostBuilder(args)
					.Build()
					// этот токен приходит в BackgroundService.ExecuteAsync
					// не нужно отдельный service
					.RunAsync(t);
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
				.UseApplicationContext()
				.UseStartup<Startup>()
				.UseConsoleLifetime();
	}
}
