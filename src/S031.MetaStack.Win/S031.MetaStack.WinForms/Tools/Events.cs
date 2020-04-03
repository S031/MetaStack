using S031.MetaStack.ORM;
using System;

namespace S031.MetaStack.WinForms
{
	//public class MetaWinEventArgs : EventArgs
	//{
	//	public MetaWinEventArgs() { }
	//	public MetaWinEventArgs(ORM.JMXObject xmlClient) => ObjSource = xmlClient;
	//	public ORM.JMXObject ObjSource { get; set; }
	//	public string ActionID { get; set; }
	//}
	public class ActionEventArgs : EventArgs
	{
		public ActionEventArgs() { }
		public ActionEventArgs(JMXObject jObject) { ObjSource = jObject; }
		public JMXObject ObjSource { get; set; }
		public string ActionID { get; set; }
	}

	public class MetaWinObjectEventArgs : EventArgs
	{
		public object ObjSource { get; set; }
		public MetaWinObjectEventArgs() { }
		public MetaWinObjectEventArgs(object source) => ObjSource = source;
	}

	public class SchemaEventArgs : EventArgs
	{
		public SchemaEventArgs() { }
		public SchemaEventArgs(JMXSchema schema) { ObjSchema = schema; }
		public JMXSchema ObjSchema { get; set; }
		public string ActionID { get; set; }
	}

	public class SchemaExceptionEventArgs : SchemaEventArgs
	{
		private readonly Connectors.TCPConnectorException _exception;

		public Connectors.TCPConnectorException Exception => _exception;

		public SchemaExceptionEventArgs(Connectors.TCPConnectorException e, JMXSchema objSource)
			: base(objSource) => _exception = e;
	}
}
