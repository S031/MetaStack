using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace S031.MetaStack.Core.App
{
	public class AppConfig: IAppConfig
	{
		JObject _config;
		public AppConfig(string source)
		{
			_config = JObject.Parse(source);
		}

		/// <summary>
		/// Access to the config element by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns><see cref="JToken"/></returns>
		public JToken this[string index] { get => _config[index]; }

		/// <summary>
		/// Provides a method to query LINQ to JSON using a single string path
		/// <see cref="JToken.SelectToken(string)"/>
		/// </summary>
		/// <param name="path">string path</param>
		/// <returns>Object config item</returns>
		/// <example>var value = config.GetConfigItem("SectionName.SubSectionName.ParmName");</example>
		public object GetConfigItem(string path) => _config.SelectToken(path);
	}
}
