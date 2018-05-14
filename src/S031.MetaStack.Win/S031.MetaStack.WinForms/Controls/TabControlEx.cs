using System;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	class TabControlEx : TabControl
	{
		private bool _headerVisible = true;
		public bool HeadersVisible
		{
			get { return _headerVisible; }
			set
			{
				_headerVisible = value;
				redraw();
			}
		}

		protected override void WndProc(ref Message m)
		{
			bool visible = DesignMode || HeadersVisible;
			if (m.Msg == 0x1328 && !visible) m.Result = (IntPtr)1;
			else base.WndProc(ref m);
		}
		private void redraw()
		{
			if (this.Visible)
			{
				this.SuspendLayout();
				this.Height -= 1;
				this.Height += 1;
				this.ResumeLayout(false);
			}
		}
	}
}
