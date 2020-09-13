using S031.MetaStack.Json;

namespace S031.MetaStack.Actions
{
	public abstract class InterfaceInfo: JsonSerializible
	{
		public InterfaceInfo(JsonValue value):base(value)
		{
		}
		public string InterfaceID { get; set; }
		public string InterfaceName { get; set; }
		public string Description { get; set; }
		public bool MultipleRowsParams { get; set; }
		public bool MultipleRowsResult { get; set; }
		public ParamInfoList InterfaceParameters { get; private set; } = new ParamInfoList();

	}
}
