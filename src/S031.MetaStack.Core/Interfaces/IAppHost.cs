using Newtonsoft.Json.Linq;
using S031.MetaStack.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace S031.MetaStack.Core.App
{
	public interface IAppHost : IDisposable
	{
		string AppServerName { get; }
		IAppConfig AppConfig { get; }
		ILogger Log { get; }
		IEnumerable<IAppService> GetServices();
		void Run();
		void Run(CancellationToken token);
	}
	public interface IAppService
	{
		Task StartAsync(IAppHost host, AppServiceOptions options);
		Task StopAsync();
		void Stop();
		bool IsRuning();
	}
	public class AppServiceOptions
	{
		CancellationToken _token = CancellationToken.None;
		/// <summary>
		/// Query period for timer if this used
		/// </summary>
		public int Delay { get; set; }
		/// <summary>
		/// Log file name
		/// </summary>
		public string LogName { get; set; }
		/// <summary>
		/// Log setiings <see cref="FileLogSettings"/>
		/// </summary>
		public FileLogSettings LogSettings { get; set; }
		/// <summary>
		/// User name
		/// </summary>
		public string UserName { get; set; }
		/// <summary>
		/// User passwor
		/// </summary>
		public string Password { get; set; }
		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/>
		/// </summary>
		public CancellationToken CancellationToken { get => _token; set => _token = value; }
	}

	public interface IAppConfig
	{
		/// <summary>
		/// Access to the config element by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns><see cref="JToken"/></returns>
		JToken this[string index] { get; }

		/// <summary>
		/// Provides a method to query LINQ to JSON using a single string path
		/// <see cref="JToken.SelectToken(string)"/>
		/// </summary>
		/// <param name="path">string path</param>
		/// <returns>Object config item</returns>
		/// <example>var value = config.GetConfigItem("SectionName.SubSectionName.ParmName");</example>
		object GetConfigItem(string path);
	}
	public interface IAppEvaluator
	{
		Data.DataPackage Invoke(Data.DataPackage dp);
	}
}
