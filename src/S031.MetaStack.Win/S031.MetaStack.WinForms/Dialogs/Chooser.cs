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
			WinFormItem selector = new WinFormItem("RadioGropPanel")
			{
				PresentationType = typeof(TableLayoutPanel),
				ControlTrigger = (c, e) =>
				{
					e.Dock = DockStyle.Fill;
				}
			};
			int i = 0;
			foreach (var item in items)
			{
				selector.Add(new WinFormItem($"P{i}")
				{
					PresentationType = typeof(RadioButton),
					ControlTrigger = (cdi, ctrl) =>
					{
						RadioButton rb = (ctrl as RadioButton);
						rb.UseVisualStyleBackColor = true;
						rb.Text = item.Value.ToString();
						rb.Tag = item.Key;
						rb.Dock = DockStyle.Top;
						rb.MouseClick += (c, e) => cdi
							.LinkedControl
							.FindForm()
							.DialogResult = System.Windows.Forms.DialogResult.OK;
						rb.MouseMove += (c, e) => rb.Cursor = Cursors.Hand;
						rb.TabIndex = i;
					}
				});
				i++;
			}

			using (WinForm cd = new BaseViewForm(selector))
			{
				cd.GetControl<RadioButton>($"P{selected}").Checked = true;
				Button btn = cd.GetControl<Button>("OK");
				cd.AcceptButton = btn;

				cd.StartPosition = FormStartPosition.CenterParent;
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
