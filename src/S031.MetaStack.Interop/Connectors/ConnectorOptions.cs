using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Interop.Connectors
{
	public class ConnectorOptions: JsonSerializible
	{
		public string Host { get; set; } = "localhost";
		public int Port { get; set; } = 8001;
		public string UID { get; set; } = $@"{Environment.UserDomainName}\{Environment.UserName}";
		public string Password { get; set; }
		public bool ForcePassword { get; set; } = false;
		public bool SavePassword { get; set; } = true;
		public Func<string, string> SecureRequest { get; set; } = s => string.Empty;

		public ConnectorOptions() : base(null)
		{
		}

		public ConnectorOptions(JsonValue config) : base(config)
		{
		}
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("Host", Host);
			writer.WriteProperty("Port", Port);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("Password", Password);
			writer.WriteProperty("ForcePassword", ForcePassword);
			writer.WriteProperty("SavePassword", SavePassword);
		}
		public override void FromJson(JsonValue source)
		{
			JsonObject j = (source as JsonObject);
			if (j.ContainsKey("Host")) Host = j["Host"];
			if (j.ContainsKey("Port")) Port = j["Port"];
			if (j.ContainsKey("UID")) UID = j["UID"];
			if (j.ContainsKey("Password")) Password = j["Password"];
			if (j.ContainsKey("ForcePassword")) ForcePassword = j["ForcePassword"];
			if (j.ContainsKey("SavePassword")) SavePassword = j["SavePassword"];
		}
	}
}
