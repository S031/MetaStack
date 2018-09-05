#if NETCOREAPP
namespace S031.MetaStack.Core.Actions
#else
namespace S031.MetaStack.WinForms.Actions
#endif
{
	public class InterfaceInfo
	{
		public InterfaceInfo()
		{
			InterfaceParameters = new ParamInfoList();
		}
		public string InterfaceID { get; set; }
		public string InterfaceName { get; set; }
		public string Description { get; set; }
		public bool MultipleRowsParams { get; set; }
		public bool MultipleRowsResult { get; set; }
		public ParamInfoList InterfaceParameters { get; private set; }
	}
}
