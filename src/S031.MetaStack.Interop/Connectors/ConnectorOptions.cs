using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Interop.Connectors
{
	public class ConnectorOptions: JsonSerializible
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string UID { get; set; }
		public string Password { get; set; }


		public ConnectorOptions(string jsonConfig)
			: this((JsonValue)new JsonReader(NullTest(jsonConfig, nameof(jsonConfig))).Read())
		{
		}

		ConnectorOptions(JsonValue config) : base(config)
		{
			JsonObject j = (config as JsonObject);
			Host = j.GetStringOrDefault("Host");
			Port = j.GetIntOrDefault("Port");
			UID = j.GetStringOrDefault("UID");
			Password = j.GetStringOrDefault("Password");
		}

		public override void ToJson(JsonWriter writer)
		{
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
		}

		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("Host", Host);
			writer.WriteProperty("Port", Port);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("Password", Password);
		}

		private static string NullTest(string data, string name)
		{
			data.NullTest(name);
			return data;
		}
	}
}
