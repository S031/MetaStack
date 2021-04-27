using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Data;
using S031.MetaStack.Security;
using S031.MetaStack.Actions;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Caching;
using S031.MetaStack.Core.Actions;

namespace S031.MetaStack.Core.App
{
	public static class ApplicationContext
    {
		private static IServiceProvider _lastBuildServiceProvider = null;
		private static IServiceCollection _services;
		private static int _lastBuildHash = 0;
		private static readonly object obj4Lock = new();

		private static MdbContext _schemaDb = null;

		/// <summary>
		/// Point to configure Core services
		/// </summary>
		/// <param name="hostBuilder"><see cref="IHostBuilder"/></param>
		/// <param name="token"><see cref="System.Threading.CancellationToken"/></param>
		/// <returns><see cref="IHost"/></returns>
		public static IHostBuilder UseApplicationContext(this IHostBuilder hostBuilder)
			=> hostBuilder.ConfigureServices(services => Configure(services));

		
		/// <summary>
		/// Возможно это лучее место для включения сервисов используемых в данной библиотеке
		/// типа LoginProvider, AuthorizationProvider, MdbFactory и т.д., кт сейчас объявлены в startup
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		private static IServiceCollection Configure(IServiceCollection services)
		{
			/// Don't use transient for services that uses SQLite connections
			_services = services;
			services.AddSingleton<IMdbContextFactory, MdbContextFactory>();
			services.AddSingleton<IActionManager, ActionManager>();
			services.AddSingleton<IAuthorizationProvider, BasicAuthorizationProvider>();
			services.AddSingleton<ILoginProvider, BasicLoginProvider>();
			services.AddSingleton<PipeQueue, PipeQueue>();
			services.AddSingleton<IUserManager, UserManager>();
			services.AddSingleton<ICacheManager, CacheManager>();
			return _services;
		}

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

		private static MdbContext SchemaDb
		{
			get
			{
				if (_schemaDb == null)
				{
					lock (obj4Lock)
					{
						var s = _lastBuildServiceProvider;
						var c = s.GetRequiredService<IConfiguration>();
						_schemaDb = s.GetRequiredService<MdbContextFactory>()
							.GetContext(c["appSettings:SysCatConnection"]);
					}
				}
				return _schemaDb;
			}

		}
    }
}
