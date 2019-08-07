using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using S031.MetaStack.Core.App;

namespace MetaStack.Test
{
	public static class Program
	{
		private static IHost _host;

		public static void ConfigureTests()
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			_host = new HostBuilder()
				.UseConsoleLifetime()
				.UseApplicationContext(configuration)
				.Build();
		}

	}
}
