using System;
using System.Windows.Forms;
using S031.MetaStack.WinForms.ORM;

namespace S031.MetaStack.WinForms
{
	public interface IObjectEditor : IObjectBase
	{
		event EventHandler<ActionEventArgs> DataSaved;
		JMXObject EditObject { get; set; }
		Form Owner { get; set; }
		IObjectHost ObjectHost { get; set; }
		bool IsNew { get; set; }
		bool SaveInDB { get; set; }
		bool TrueSaved { get; set; }
		bool ContinuousEditing { get; set; }
		bool RefreshHostOnExit { get; set; }
		bool RefreshHostOnSaved { get; set; }
		DialogResult ShowDialog();
		void Show();
	}
}
