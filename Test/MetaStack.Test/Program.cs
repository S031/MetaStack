using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MetaStack.Test
{
    public static class Program
    {
		static IHost _host;

		public static void ConfigureTests()
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			_host = new HostBuilder()
				.UseConsoleLifetime()
				.ConfigureServices((context, services) => services
					.AddSingleton<IConfiguration>(configuration)
					.AddSingleton<ILoginFactory>(new BasicLoginFactory()))
				.UseApplicationContext()
				.Build();
		}

	}
}
