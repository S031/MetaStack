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
			if (o.ContainsKey("AttribName")) AttribName = o["AttribName"];
			if (o.ContainsKey("AttribPath")) AttribPath = o["AttribPath"];
			if (o.ContainsKey("Name")) Name = o["AttribPath"];
			if (o.ContainsKey("DataType")) DataType = o["DataType"];
			if (o.ContainsKey("Mask")) Mask = o["Mask"];
			if (o.ContainsKey("Format")) Format = o["Format"];
			if (o.ContainsKey("Position")) Position = o["Position"];
			if (o.ContainsKey("Width")) Width = o["Width"];
			if (o.ContainsKey("DisplayWidth")) DisplayWidth = o["DisplayWidth"];
			if (o.ContainsKey("SuperForm")) SuperForm = o["SuperForm"];
			if (o.ContainsKey("SuperObject")) SuperObject = o["SuperObject"];
			if (o.ContainsKey("SuperMethod")) SuperMethod = o["SuperMethod"];
			if (o.ContainsKey("SuperFilter")) SuperFilter = o["SuperFilter"];
			if (o.ContainsKey("ListItems")) ListItems = o["ListItems"];
			if (o.ContainsKey("ListData")) ListData = o["ListData"];
			if (o.ContainsKey("FieldName")) FieldName = o["FieldName"];
			if (o.ContainsKey("ConstName")) ConstName = o["ConstName"];
			if (o.ContainsKey("Agregate")) Agregate = o["Agregate"];
			if (o.ContainsKey("IsPK")) IsPK = o["IsPK"];
			if (o.ContainsKey("Locate")) Locate = o["Locate"];
			if (o.ContainsKey("Visible")) Visible = o["Visible"];
			if (o.ContainsKey("ReadOnly")) ReadOnly = o["ReadOnly"];
			if (o.ContainsKey("Enabled")) Enabled = o["Enabled"];
			if (o.ContainsKey("Sorted")) Sorted = o["Sorted"];
		}
	}
}
