using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;


namespace S031.MetaStack.Core.SysCat
{
    internal static class SysCatItems
    {
		public static readonly Dictionary<string, string> schemas = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

		static SysCatItems()
		{
			schemas.Add("SysSequence", new JObject(
				new JProperty("ClassIName", "SysSequence"),
				new JProperty("ClassType", "Class"),
				new JProperty("Name", "SysSequence"),
				new JProperty("TableName", "SysSequence"),
				new JProperty("Attributes",
					new JArray(
						new JObject(
							new JProperty("AttribName", "SysID"),
							new JProperty("Name", "Идентификатор"),
							new JProperty("IsPK", 1),
							new JProperty("IsFK", 0),
							new JProperty("NullOption", "NOT NULL"),
							new JProperty("NullIfDefault", 0),
							new JProperty("DataType", "bigint"),
							new JProperty("Width", 8),
							new JProperty("ClientDataType", "long"),
							new JProperty("IsObject", false),
							new JProperty("IsArray", false),
							new JProperty("Identity", 0),
							new JProperty("ReadOnly", 1),
							new JProperty("Enabled", 1),
							new JProperty("ClientDefault", 0),
							new JProperty("ServerDefault", 0),
							new JProperty("ListItems", ""),
							new JProperty("ListData", ""),
							new JProperty("Format", ""),
							new JProperty("Mask", "")
						),
						new JObject(
							new JProperty("AttribName", "SysUID"),
							new JProperty("Name", "Идентификатор UID"),
							new JProperty("IsPK", 0),
							new JProperty("IsFK", 0),
							new JProperty("NullOption", "NOT NULL"),
							new JProperty("NullIfDefault", 0),
							new JProperty("DataType", "uniqueidentifier"),
							new JProperty("Width", 34),
							new JProperty("ClientDataType", "guid"),
							new JProperty("IsObject", false),
							new JProperty("IsArray", false),
							new JProperty("Identity", 0),
							new JProperty("ReadOnly", 1),
							new JProperty("Enabled", 1),
							new JProperty("ClientDefault", 0),
							new JProperty("ServerDefault", 0),
							new JProperty("ListItems", ""),
							new JProperty("ListData", ""),
							new JProperty("Format", ""),
							new JProperty("Mask", "")
						)
					)
				),
				new JProperty("PrimaryKey",
					new JObject(
						new JProperty("Name", "PK_SYSSEQUENCE"),
						new JProperty("Members",
							new JArray(
								"SysID"
								)
						)
					)
				)
			).ToString(Newtonsoft.Json.Formatting.None));
		}
	}
}
