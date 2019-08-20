using System;
using S031.MetaStack.Common;
using fastJSON;
using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.Json
#else
namespace S031.MetaStack.WinForms.Json
#endif
{
	public static class JSONExtensions
    {
		private static readonly JSONParameters _jsonParameters = new JSONParameters()
		{
			SerializeNullValues = false, UseExtensions = true
		};
 
		public static object DeserializeObject(string json)
			=> fastJSON.JSON.ToObject(json, _jsonParameters);

		public static T DeserializeObject<T>(string json)
			=> fastJSON.JSON.ToObject<T>(json, _jsonParameters);

		public static string SerializeObject(object value)
			=> new JsonWriter(Formatting.None)
			.WriteValue(new JsonValue(value))
			.ToString();
			//=> fastJSON.JSON.ToJSON(value, _jsonParameters);
	}
}
