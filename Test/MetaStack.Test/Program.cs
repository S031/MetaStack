using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Caching;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.Reflection;

namespace MetaStack.Test
{
	internal static class Program
	{
		private static IHost _host;

		public static IServiceProvider GetServices()
		{
			if (_host == null)
			{
				_host = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(config =>
				{
					config.AddJsonFile("config.json", optional: false, reloadOnChange: false);
				})
				.ConfigureLogging(logging =>
				{
					logging
						.ClearProviders()
						.AddFile();
				})
				.ConfigureServices(s=>
					s.AddSingleton(p => p.GetRequiredService<ILoggerProvider>()
						.CreateLogger(Assembly.GetEntryAssembly().GetWorkName()))
				)
				.UseConsoleLifetime()
				.Build(new System.Threading.CancellationToken());
			}
			return _host.Services;
		}
	}
}
