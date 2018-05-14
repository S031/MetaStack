using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public class ToolStripDateEdit: ToolStripControlHost
	{
		public ToolStripDateEdit()
			: base(new DateEdit())
		{
			((DateEdit)Control).FlatStyle = FlatStyle.Popup;
		}
		public DateTime Value
		{
			get { return ((DateEdit)Control).Value; }
			set { ((DateEdit)Control).Value = value; }
		}
	}
}
