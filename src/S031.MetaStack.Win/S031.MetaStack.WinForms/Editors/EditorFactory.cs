using System;
using System.Windows.Forms;
using S031.MetaStack.ORM;

namespace S031.MetaStack.WinForms
{
	public static class EditorFactory
	{
		public static void ObjectEdit(JMXObject objSource, bool isNew)
		{
			ObjectEdit(objSource, isNew, null, null, null, null);
		}

		public static void ObjectEdit(JMXObject objSource, bool isNew, IObjectHost host)
		{
			ObjectEdit(objSource, isNew, host, null, null, null);
		}

		public static void ObjectEdit(JMXObject objSource, bool isNew, IObjectHost host, IObjectEditor editor)
		{
			ObjectEdit(objSource, isNew, host, null, null, editor);
		}

		public static void ObjectEdit(JMXObject objSource, bool isNew,
			IObjectHost host, IObjectEditor editor, Action<IObjectEditor> RefreshHostDelegate)
		{
			ObjectEdit(objSource, isNew, host, null, RefreshHostDelegate, editor);
		}

		public static void ObjectEdit(JMXObject objSource, bool isNew, IObjectHost host,
			string actionID, Action<IObjectEditor> RefreshHostDelegate, IObjectEditor editor)
		{
			IObjectEditor oe;
			if (editor == null)
				editor = new MultiEdit(objSource.ObjectName);
			oe = editor;
			oe.IsNew = isNew;
			oe.EditObject = objSource;

			if (oe.EditObject != null)
			{
				oe.Owner = host == null ? null : (host as Control).FindForm();
				oe.ObjectHost = host;
				oe.DataSaved += (sender, e) =>
				{
					IObjectEditor _oe = (sender as IObjectEditor);
					if (_oe != null && _oe.TrueSaved && _oe.RefreshHostOnSaved && RefreshHostDelegate != null)
						RefreshHostDelegate(oe);
				};
				oe.ContinuousEditing = true;

				Form form = (oe as Form);
				if (form != null)
					form.FormClosed += (sender, e) =>
					{
						IObjectEditor _oe = (sender as IObjectEditor);
						if (_oe != null && _oe.TrueSaved && _oe.RefreshHostOnExit && RefreshHostDelegate != null)
							RefreshHostDelegate(oe);
						Control _host = (_oe.ObjectHost as Control);
						if (_host != null) _host.Focus();
					};

				if (oe.Owner != null && !oe.Owner.Modal)
					form.Show();
				else
				{
					form.ShowDialog();
					form.Dispose();
				}
			}
		}
	}
}
