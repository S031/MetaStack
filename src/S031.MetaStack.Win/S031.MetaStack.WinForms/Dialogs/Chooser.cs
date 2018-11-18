using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public static class Chooser
	{
		public static string Choose(IEnumerable<string> items)
		{
			return Choose(items.Select(Item=>new KeyValuePair<string, string>(Item, Item)), 0);
		}

		public static TKey Choose<TKey, TText>(IEnumerable<KeyValuePair<TKey, TText>> items, int selected)
		{
			using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			{
				cd.Add<Panel>(WinFormConfig.SinglePageForm);
				TableLayoutPanel tlpRows = cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
				TableLayoutPanel tp = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(1, 1) });
				int i = 0;
				foreach (var item in items)
				{
					tp.Add(new WinFormItem($"P{i}")
					{
						PresentationType = typeof(RadioButton),
						ControlTrigger = (cdi, ctrl) =>
						{
							RadioButton rb = (ctrl as RadioButton);
							rb.UseVisualStyleBackColor = true;
							rb.Text = item.Value.ToString();
							rb.Tag = item.Key;
							rb.Dock = DockStyle.Top;
							rb.Checked = (i == selected);
							rb.MouseClick += (c, e) => cd.DialogResult = System.Windows.Forms.DialogResult.OK;
							rb.MouseMove += (c, e) => rb.Cursor = Cursors.Hand;
						}
					});
					i++;
				}
				cd.Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons("&Выбор"));
				Button btn = cd.GetControl<Button>("OK");
				cd.AcceptButton = btn;

				cd.StartPosition = FormStartPosition.CenterParent;
				cd.Height = tp.Height + 100;
				if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					cd.Save();
					return (TKey)cd.Items
						.Select(kvp => kvp.Value.LinkedControl as RadioButton)
						.FirstOrDefault(rb => rb != null && rb.Checked).Tag;
				}
				else
					return default;
			}
		}
	}
}
