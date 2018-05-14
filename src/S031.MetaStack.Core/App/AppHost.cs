using System;
using System.Collections.Generic;
using System.Threading;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Threading.Tasks;
using S031.MetaStack.Core.Json;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Logging;

namespace S031.MetaStack.Core.App
{
	public class AppHost : IAppHost, IDisposable
	{
		const string serverName = "S031.MetaStack.AppServer";
		private readonly string _configName;
		private IAppConfig _config;
		private PluginManager _pm;
		private ILogger _log;
		private List<IAppService> _srvs = new List<IAppService>();
		private CancellationToken _token;

		public string AppServerName => serverName;

		/// <summary>
		/// Creates a new instance of<see cref="AppHost"/> with specified config file name
		/// </summary>
		/// <param name="configName">config file name</param>
		/// <exception cref="System.IO.FileNotFoundException"/>
		public AppHost(string configName)
		{
			configName.NullTest(nameof(configName));
			if (!System.IO.File.Exists(configName))
				//The config.json configuration file was not found
				throw new System.IO.FileNotFoundException("S031.MetaStack.Core.App.AppHost.1".GetTranslate());
			_configName = configName;
		}

		/// <summary>
		/// Get <see cref="IAppConfig"/> reference
		/// </summary>
		public IAppConfig AppConfig { get => _config; }

		/// <summary>
		/// Get <see cref="ILogger"/> reference
		/// </summary>
		public ILogger Log { get => _log; }

		void IDisposable.Dispose()
		{
			stopServices();
			(_log as IDisposable)?.Dispose();
		}
		public IEnumerable<IAppService> GetServices()
		{
			foreach (var s in _srvs)
				yield return s;
		}

		public void Run()
		{
			Run(new CancellationTokenSource().Token);
		}

		public void Run(CancellationToken token = default)
		{
			_token = token;
			loadConfig();
			//_token.WaitHandle.WaitOne();
		}
		void loadConfig()
		{
			_config = new AppConfig(System.IO.File.ReadAllText(_configName));
			configureLog();
			_pm = new PluginManager(this);
			configureServices();
			_log.LogInformation($"AppHost started with configuration file {_configName}");
		}
		void configureServices()
		{
			var serviceSection = (_config.GetConfigItem("IAppServiceConfiguration.ImplementationList") as JArray);
			if (serviceSection != null)
			{
				foreach (var serviceItem in serviceSection)
				{
					Assembly a = _pm.LoadAssembly((string)serviceItem["assemblyName"]);
					var instance = _pm.CreateInstance<IAppService>((string)serviceItem["typeName"]);
					var options = new AppServiceOptions();
					if (serviceItem["delay"] != null)
						options.Delay = (int)serviceItem["delay"];
					if (serviceItem["userName"] != null)
						options.UserName = (string)serviceItem["userName"];
					if (serviceItem["password"] != null)
						options.Password = (string)serviceItem["password"];
					options.LogSettings = null;
					if (serviceItem["logName"] != null && !((string)serviceItem["logName"]).IsEmpty())
					{
						options.LogName = (string)serviceItem["logName"];
						options.LogSettings = JSONExtensions.DeserializeObject<FileLogSettings>(serviceItem["logSettings"].ToString());
					}
					else
						options.LogName = string.Empty;
					options.CancellationToken = _token;
					//instance.Start(this, options);
					var t = instance.StartAsync(this, options);
					_srvs.Add(instance);
				}
			}
		}

		void configureLog()
		{
			var logSection = _config["ApplicationLogSettings"];
			FileLogSettings.Default.CacheSize = (int)(logSection["CacheSize"] ?? FileLogSettings.Default.CacheSize);
			FileLogSettings.Default.DateFolderMask = (string)(logSection["DateFolderMask"] ?? FileLogSettings.Default.DateFolderMask);
			LogLevels logLevel = LogLevels.Information;
			if (logSection["LogLevel"] != null)
			{
				if (Enum.TryParse(typeof(LogLevels), (string)logSection["LogLevel"], out var level))
					logLevel = (LogLevels)level;
				FileLogSettings.Default.Filter = (s, l) => l >= logLevel;
			}
			if (logSection["LevelToFlush"] != null)
			{
				if (Enum.TryParse(typeof(LogLevels), (string)logSection["LevelToFlush"], out var level))
					FileLogSettings.Default.LevelToFlush = (LogLevels)level;
			}
			_log = new Logging.FileLogger(serverName);
			_log.Debug($"Log created with the specified settings:\n" +
				$"CacheSize={FileLogSettings.Default.CacheSize},\n" +
				$"DateFolderMask = {FileLogSettings.Default.DateFolderMask},\n" +
				$"LogLevel={logLevel},\n" +
				$"LevelToFlush={FileLogSettings.Default.LevelToFlush}");

		}
		void stopServices()
		{
			//List<Task> tl = new List<Task>();
			//foreach (var service in _srvs)
			//	tl.Add(service.StopAsync());
			//Task.WaitAll(tl.ToArray());
			foreach (var service in _srvs)
			{
				//var t = service.StopAsync();
				service.Stop();
			}
		}
	}
	static class taskEx
	{
		public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
		{
			return task.IsCompleted
				? task
				: task.ContinueWith(
					completedTask => completedTask.GetAwaiter().GetResult(),
					cancellationToken,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
		}
	}
}
