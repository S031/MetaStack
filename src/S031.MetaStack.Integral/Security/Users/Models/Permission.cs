using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Integral.Security
{
	public class Permission : JsonSerializible
	{
		public string SchemaName { get; set; }
		public string ObjectName { get; set; }
		public string ActionID { get; set; }
		public bool IsGranted { get; set; }

		public Permission() : base(null) { }
		public Permission(JsonValue source) : base(source) { }
		public override void FromJson(JsonValue source)
		{
			JsonObject j = (source as JsonObject);
			SchemaName = j.GetStringOrDefault("SchemaName");
			ObjectName = j.GetStringOrDefault("ObjectName");
			ActionID = j.GetStringOrDefault("ActionID");
			IsGranted = j.GetBoolOrDefault("IsGranted");
		}
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("SchemaName", SchemaName);
			writer.WriteProperty("ObjectName", ObjectName);
			writer.WriteProperty("ActionID", ActionID);
			writer.WriteProperty("IsGranted", IsGranted);
		}
	}
}
