namespace S031.MetaStack.ORM.Actions
{
	public class AttribInfo
	{
		public AttribInfo()
		{
			Locate = true;
			DataType = "String";
			Width = 30;
			DisplayWidth = 10;
			AttribName = string.Empty;
			AttribPath = string.Empty;
			Name = string.Empty;
			Position = 0;
			Mask = string.Empty;
			Format = string.Empty;
			IsPK = false;
			Visible = true;
			ReadOnly = false;
			Enabled = true;
			Sorted = false;
			SuperMethod = string.Empty;
			SuperFilter = string.Empty;
			SuperObject = string.Empty;
			ListItems = string.Empty;
			ListItems = string.Empty;
			FieldName = string.Empty;
			ConstName = string.Empty;
			Agregate = string.Empty;
		}
		public string AttribName { get; set; }
		public string AttribPath { get; set; }
		public string Name { get; set; }
		public int Position { get; set; }
		public string DataType { get; set; }
		public int Width { get; set; }
		public int DisplayWidth { get; set; }
		public string Mask { get; set; }
		public string Format { get; set; }
		public bool IsPK { get; set; }
		public bool Locate { get; set; }
		public bool Visible { get; set; }
		public bool ReadOnly { get; set; }
		public bool Enabled { get; set; }
		public bool Sorted { get; set; }
		public string SuperForm { get; set; }
		public string SuperObject { get; set; }
		public string SuperMethod { get; set; }
		public string SuperFilter { get; set; }
		public string ListItems { get; set; }
		public string ListData { get; set; }
		public string FieldName { get; set; }
		public string ConstName { get; set; }
		public string Agregate { get; set; }
	}
}
