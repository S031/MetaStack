using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public class BaseViewForm: WinForm
	{
		public BaseViewForm(WinFormItem browser)
		{
			Width = 680;
			Height = (int)(Width / vbo.GoldenRatio);
			this.Add<Panel>(WinFormConfig.SinglePageForm);
			TableLayoutPanel tp = Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			tp.Add(browser);
			browser.LinkedControl.Dock = DockStyle.Fill;
			var btp = GetItem("MainPanel")
				.LinkedControl
				.Add<TableLayoutPanel>(WinFormConfig.StdButtons());
			btp.Height = ButtonHeight + 20;
		}
	}
}
