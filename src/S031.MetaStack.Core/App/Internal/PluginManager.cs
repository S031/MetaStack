using System.Reflection;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Logging;

namespace S031.MetaStack.Core.App
{
	/// <summary>
	/// loader for program extensions and their dependencies
	/// </summary>
	internal class PluginManager
	{
		//list of required interfaces
		private readonly IList<Type> _interfaceList = new List<Type>();
		///application host <see cref="IAppHost"/>
		private IAppHost _appHost;
		private ILogger _log;

		/// <summary>
		/// Creates a new instance of<see cref="PluginManager"/> and load  list of types of required interfaces
		/// </summary>
		/// <param name="host"></param>
		internal PluginManager(IAppHost host)
		{
			_appHost = host;
			_log = host.Log;
			_log.Debug($"PluginManager for host {_appHost.AppServerName} creating");
			var interfaceSection = (host.AppConfig.GetConfigItem("pluginManagerConfiguration.InterfacesList") as JArray);
			if (interfaceSection != null)
			{
				foreach (var interfaceItem in interfaceSection)
				{
					Assembly a = Assembly.Load((string)interfaceItem["assemblyName"]);
					Type t = a.GetType((string)interfaceItem["typeName"]);
					_interfaceList.Add(t);
					ImplementsList.Add(t);
					_log.Debug($"The required interface {(string)interfaceItem["typeName"]} from Pluin Assembly {a.FullName} was added");
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public T CreateInstance<T>(string typeName)
		{
			return (T)ImplementsList.GetTypes(typeof(T)).FirstOrDefault(t => t.FullName == typeName).CreateInstance();
		}

		public Assembly LoadAssembly(string assemblyID)
		{
			Assembly a = AssemblyLoadContext.Default.LoadFromAssemblyPath($"{System.AppContext.BaseDirectory}/{assemblyID}.dll");
			foreach (var t in _interfaceList)
				ImplementsList.Add(t, a);
			return a;
		}
	}
}
