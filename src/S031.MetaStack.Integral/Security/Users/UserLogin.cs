using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Integral.Security
{
	public class UserLogin: JsonSerializible
	{
		public int UserID { get; private set; }
		public string LoginProvider { get; set; }
		public string ProviderKey { get; set; }
		public string ProviderDisplayName { get; set; }

		public UserLogin(JsonValue source) : base(source) { }
		public override void FromJson(JsonValue source)
		{
			JsonObject j = (source as JsonObject);
			UserID = j.GetIntOrDefault("UserID");
			LoginProvider = j.GetStringOrDefault("LoginProvider");
			ProviderKey = j.GetStringOrDefault("ProviderKey");
			ProviderDisplayName = j.GetStringOrDefault("ProviderDisplayName");
		}
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("UserID", UserID);
			writer.WriteProperty("LoginProvider", LoginProvider);
			writer.WriteProperty("ProviderKey", ProviderKey);
			writer.WriteProperty("ProviderDisplayName", ProviderDisplayName);
		}
	}
}
