using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

namespace S031.MetaStack.Core.App
{
    public static class ApplicationContext
    {
		static IServiceCollection _services;
		static IServiceProvider _lastBuildServiceProvider = null;
		static int _lastBuildHash = 0;
		static readonly object obj4Lock = new object();
		static readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public static IHostBuilder UseApplicationContext(this IHostBuilder host)
		{
			host.ConfigureServices(services => Configure(services));
			return host;
		}

		private static IServiceCollection Configure(IServiceCollection services)
		{
			services.AddSingleton<CancellationTokenSource>(_cts);
			_services = services;
			return services;
		}
		public static IServiceCollection Services => _services;

		public static IServiceProvider GetServices(ServiceProviderOptions options = default)
		{
			int hash = new {ServiceCount = _services.Count, ValidateScopes = (options != null && options.ValidateScopes) }.GetHashCode();
			if (hash == _lastBuildHash)
				return _lastBuildServiceProvider;
			lock (obj4Lock)
			{
				if (options == null)
					_lastBuildServiceProvider = _services.BuildServiceProvider();
				else
					_lastBuildServiceProvider = _services.BuildServiceProvider(options);
				_lastBuildHash = hash;
			}
			return _lastBuildServiceProvider;
		}

		public static CancellationToken CancellationToken => _cts.Token;
    }
}
