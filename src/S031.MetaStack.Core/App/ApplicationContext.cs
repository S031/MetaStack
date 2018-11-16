using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Common.Logging;
using System.Data.Common;
using System.Reflection;
using System.Runtime.Loader;
using S031.MetaStack.Common;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Actions;

namespace S031.MetaStack.Core.App
{
    public static class ApplicationContext
    {
		static IServiceProvider _lastBuildServiceProvider = null;
		static int _lastBuildHash = 0;

		static readonly object obj4Lock = new object();
		static readonly CancellationTokenSource _cts = new CancellationTokenSource();
		static IServiceCollection _services;
		static IConfiguration _configuration;
		static ILogger _logger;
		static ILoginFactory _loginFactory;
		static MdbContext _schemaDb = null;
		static readonly PipeService _pipeChannel = new PipeService();

		public static IConfiguration GetConfiguration() => _configuration;
		public static ILogger GetLogger() => _logger;
		public static ILoginFactory GetLoginFactory() => _loginFactory;

		public static IHostBuilder UseApplicationContext(this IHostBuilder host, IConfiguration configuration)
		{
			_configuration = configuration;
			host.ConfigureServices(services => Configure(services));
			AppDomain.CurrentDomain.ProcessExit += DisposeMe;
			return host;
		}

		private static void DisposeMe(object sender, EventArgs e)
		{
			(_loginFactory as IDisposable)?.Dispose();
			(_logger as IDisposable)?.Dispose();
		}

		private static IServiceCollection Configure(IServiceCollection services)
		{
			_services = services;
			_services
				.AddSingleton<CancellationTokenSource>(_cts)
				.AddSingleton<IConfiguration>(_configuration);
			ConfigureLogging();
			ConfigureLoginFactory();
			ConfigureServicesFromConfigFile();
			ConfigureProvidersFromConfigFile();
			ConfigureDefaultsFromConfigFile();
			return _services;
		}

		private static IServiceCollection ConfigureLogging()
		{
			var logSettings = _configuration.GetSection("ApplicationLogSettings")?.Get<Common.Logging.FileLogSettings>();
			if (logSettings == null)
				logSettings = FileLogSettings.Default;
			_logger = new FileLogger($"AppServer.{Environment.MachineName}", logSettings);
			_services.AddSingleton<ILogger>(_logger);
			return _services;
		}
		private static IServiceCollection ConfigureLoginFactory()
		{
			//костыль!!!
			//return settings from configuration
			_loginFactory = new BasicLoginFactory();
			_services.AddSingleton<ILoginFactory>(_loginFactory );
			return _services;
		}

		private static void ConfigureServicesFromConfigFile()
		{
			var serviceList = _configuration.GetSection("IAppServiceConfiguration:ImplementationList").GetChildren();
			foreach (var section in serviceList)
			{
				var options = section.Get<Core.Services.HostedServiceOptions>();
				_services.AddTransient<Core.Services.HostedServiceOptions>(s => options);
				_services.Add<IHostedService>(options.TypeName, options.AssemblyName);
			}
		}
		private static void ConfigureProvidersFromConfigFile()
		{
			//костыль!!!
			//Remove from project references all plugins and configure publish plugins to project 
			//output folder
			//Load to publish folder all plugins whis depencies (after publish plugin progect)
			var serviceList = _configuration.GetSection("Dependencies").GetChildren();
			foreach (var section in serviceList)
			{
				if (section["AssemblyPath"].IsEmpty())
					Assembly.Load(section["AssemblyName"]);
				else
					LoadAssembly(section["AssemblyPath"]);
			}
		}
		private static void ConfigureDefaultsFromConfigFile()
		{
			//костыль!!!
			//return settings from configuration
		}
		private static Assembly LoadAssembly(string assemblyID)=> AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));

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

		/// <summary>
		/// Recomended for sqlite sys cat
		/// </summary>
		/// <param name="workConnectionName"></param>
		/// <returns></returns>
		public static JMXFactory CreateJMXFactory(string workConnectionName)
		{
			var workConnectInfo = _configuration.GetSection($"connectionStrings:{workConnectionName}").Get<ConnectInfo>();
			MdbContext workDb = new MdbContext(workConnectInfo);
			var f = JMXFactory.Create(SchemaDb, workDb, _logger);
			f.IsLocalContext = true;
			return f;
		}

		public static ActionManager GetActionManager() => new ActionManager(SchemaDb) { Logger = GetLogger() };

		private static MdbContext SchemaDb
		{
			get
			{
				if (_schemaDb == null)
				{
					lock (obj4Lock)
					{
						var schemaConnectInfo = _configuration.GetSection($"connectionStrings:{_configuration["appSettings:SysCatConnection"]}").Get<ConnectInfo>();
						_schemaDb = new MdbContext(schemaConnectInfo);
					}
				}
				return _schemaDb;
			}

		}

		public static PipeService GetPipe() => _pipeChannel;
    }
}
