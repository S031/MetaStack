using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Integral.Security
{
	public class Permission : JsonSerializible
	{
		/// <summary>
		/// Name of schema object in format [SchemaName].[ObjectName] (as dbo.Accounts)
		/// </summary>
		public string ObjectName { get; set; }
		public string ActionID { get; set; }
		public string RoleID { get; set; }
		public bool IsGranted { get; set; }

		public Permission() : base(null) { }
		public Permission(JsonValue source) : base(source) { }
		public override void FromJson(JsonValue source)
		{
			JsonObject j = (source as JsonObject);
			ObjectName = j.GetStringOrDefault("ObjectName");
			ActionID = j.GetStringOrDefault("ActionID");
			RoleID = j.GetStringOrDefault("RoleID");
			IsGranted = j.GetBoolOrDefault("IsGranted");
		}
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("ObjectName", ObjectName);
			writer.WriteProperty("ActionID", ActionID);
			writer.WriteProperty("RoleID", RoleID);
			writer.WriteProperty("IsGranted", IsGranted);
		}
	}
}
