using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.App
{
    public static class ApplicationContext
    {
		static IServiceCollection _services;
		public static IHostBuilder UseApplicationContext(this IHostBuilder host)
		{
			host.ConfigureServices(services => _services = services);
			return host;
		}
		public static IServiceProvider GetServices(ServiceProviderOptions options = default)
		{
			return _services.BuildServiceProvider(options);
		}
    }
}
