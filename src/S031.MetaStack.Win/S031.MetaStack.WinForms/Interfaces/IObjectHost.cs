using System;
using System.Windows.Forms;
using S031.MetaStack.ORM;
using S031.MetaStack.WinForms.ORM;

namespace S031.MetaStack.WinForms
{
	public interface IObjectHost
	{
		bool AllowAddObject { get; set; }
		bool AllowDelObject { get; set; }
		bool AllowEditObject { get; set; }
		event EventHandler<ActionEventArgs> ObjectAdd;
		event EventHandler<ActionEventArgs> ObjectCopy;
		event EventHandler<ActionEventArgs> ObjectCut;
		event EventHandler<ActionEventArgs> ObjectDel;
		event EventHandler<ActionEventArgs> ObjectEdit;
		event EventHandler<ActionEventArgs> ObjectPaste;
		event EventHandler<ActionEventArgs> ObjectRead;
		event EventHandler<ActionEventArgs> ObjectReadNew;
		void OnObjectAdd(ActionEventArgs e);
		void OnObjectCopy(ActionEventArgs e);
		void OnObjectCut(ActionEventArgs e);
		void OnObjectDel(ActionEventArgs e);
		void OnObjectEdit(ActionEventArgs e);
		void OnObjectPaste(ActionEventArgs e);
		void OnObjectRead(ActionEventArgs e);
		void OnObjectReadNew(ActionEventArgs e);
		string ObjectName { get; set; }
		JMXObject ReadObject();
		JMXObject ReadObject(bool createNew);
		void RefreshAll(object bookmark);
		Form FindForm();
	}
}
