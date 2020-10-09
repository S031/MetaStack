using S031.MetaStack.Json;

namespace S031.MetaStack.Actions
{
	public class AttribInfo : JsonSerializible
	{
		public AttribInfo() : base(null) { }

		public string AttribName { get; set; } = string.Empty;
		public string AttribPath { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public int Position { get; set; } = 0;
		public string DataType { get; set; } = "String";
		public int Width { get; set; } = 30;
		public int DisplayWidth { get; set; } = 10;
		public string Mask { get; set; } = string.Empty;
		public string Format { get; set; } = string.Empty;
		public bool IsPK { get; set; } = false;
		public bool Locate { get; set; } = true;
		public bool Visible { get; set; } = true;
		public bool ReadOnly { get; set; } = false;
		public bool Enabled { get; set; } = true;
		public bool Sorted { get; set; } = false;
		public string SuperForm { get; set; } = string.Empty;
		public string SuperObject { get; set; } = string.Empty;
		public string SuperMethod { get; set; } = string.Empty;
		public string SuperFilter { get; set; } = string.Empty;
		public string ListItems { get; set; } = string.Empty;
		public string ListData { get; set; } = string.Empty;
		public string FieldName { get; set; } = string.Empty;
		public string ConstName { get; set; } = string.Empty;
		public string Agregate { get; set; } = string.Empty;

		protected internal AttribInfo(JsonObject o)
			: base(o)
		{
		}
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("AttribName", AttribName);
			writer.WriteProperty("AttribPath", AttribPath);
			writer.WriteProperty("Name", Name);
			writer.WriteProperty("DataType", DataType);
			writer.WriteProperty("Mask", Mask);
			writer.WriteProperty("Format", Format);
			writer.WriteProperty("Position", Position);
			writer.WriteProperty("Width", Width);
			writer.WriteProperty("DisplayWidth", DisplayWidth);
			writer.WriteProperty("SuperForm", SuperForm);
			writer.WriteProperty("SuperObject", SuperObject);
			writer.WriteProperty("SuperMethod", SuperMethod);
			writer.WriteProperty("SuperFilter", SuperFilter);
			writer.WriteProperty("ListItems", ListItems);
			writer.WriteProperty("ListData", ListData);
			writer.WriteProperty("FieldName", FieldName);
			writer.WriteProperty("ConstName", ConstName);
			writer.WriteProperty("Agregate", Agregate);
			writer.WriteProperty("IsPK", IsPK);
			writer.WriteProperty("Locate", Locate);
			writer.WriteProperty("Visible", Visible);
			writer.WriteProperty("ReadOnly", ReadOnly);
			writer.WriteProperty("Enabled", Enabled);
			writer.WriteProperty("Sorted", Sorted);
		}
		public override void FromJson(JsonValue source)
		{
			var o = source as JsonObject;
			AttribName = o.GetStringOrDefault("AttribName");
			AttribPath = o.GetStringOrDefault("AttribPath");
			Name = o.GetStringOrDefault("AttribPath");
			DataType = o.GetStringOrDefault("DataType");
			Mask = o.GetStringOrDefault("Mask");
			Format = o.GetStringOrDefault("Format");
			Position = o.GetIntOrDefault("Position");
			Width = o.GetIntOrDefault("Width");
			DisplayWidth = o.GetIntOrDefault("DisplayWidth");
			SuperForm = o.GetStringOrDefault("SuperForm");
			SuperObject = o.GetStringOrDefault("SuperObject");
			SuperMethod = o.GetStringOrDefault("SuperMethod");
			SuperFilter = o.GetStringOrDefault("SuperFilter");
			ListItems = o.GetStringOrDefault("ListItems");
			ListData = o.GetStringOrDefault("ListData");
			FieldName = o.GetStringOrDefault("FieldName");
			ConstName = o.GetStringOrDefault("ConstName");
			Agregate = o.GetStringOrDefault("Agregate");
			IsPK = o.GetBoolOrDefault("IsPK");
			Locate = o.GetBoolOrDefault("Locate");
			Visible = o.GetBoolOrDefault("Visible");
			ReadOnly = o.GetBoolOrDefault("ReadOnly");
			Enabled = o.GetBoolOrDefault("Enabled");
			Sorted = o.GetBoolOrDefault("Sorted");
		}
	}
}
