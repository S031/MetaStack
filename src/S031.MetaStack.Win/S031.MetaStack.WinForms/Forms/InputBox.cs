using S031.MetaStack.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
    public static class InputBox
	{
		public static T Show<T>(string caption, T value, T resultIfCancel)
		{
			using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			{
				T result;
				cd.Add<Panel>(WinFormConfig.SinglePageForm);
				TableLayoutPanel tlpRows = cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
				TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 1) });
				p.Add(new WinFormItem("Item")
				{
					Caption = caption,
					DataType = typeof(T),
					Value = value,
				});
				cd.Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons);
				if (cd.ShowDialog() == DialogResult.OK)
				{
					cd.Save();
					result = (T)cd.GetItem("Item").Value;
				}
				else
					result = resultIfCancel;
				return (T)result;
			}
		}
		public static Object[] Show(params WinFormItem[] items)
		{
			using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			{
				cd.Add<Panel>(WinFormConfig.SinglePageForm);
				TableLayoutPanel tlpRows = cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
				TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 1) });
				p.ColumnStyles[0].SizeType = SizeType.Percent;
				p.ColumnStyles[0].Width = 38;
				p.ColumnStyles[1].SizeType = SizeType.Percent;
				p.ColumnStyles[1].Width = 62;
				foreach (var item in items)
					p.Add(item);
				cd.Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons);
				if (cd.ShowDialog() == DialogResult.OK)
				{
					cd.Save();
					return items.Select(i=>i.Value).ToArray();
				}
				else
					return items.Select(i => i.OriginalValue).ToArray();
			}
		}

		public static void ShowCells(string caption, DataTable dataSource)
		{
			using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			{
				cd.Text = caption;
				cd.Add<Panel>(WinFormConfig.SinglePageForm);
				TableLayoutPanel tlpRows = cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
				DataGridView g = tlpRows.Add<DataGridView>(new WinFormItem("WorkCells"));
				//g.AutoGenerateColumns = false;
				//g.Columns.Add("Number", "Номер");
				//g.Columns.Add("Name", "Наименование");
				//g.Columns.Add("Data", "Данные");
				//g.DataSource = new BindingSource(dataSource, null);
				g.DataSource = dataSource;
				//g.Columns[0].DataPropertyName = "Col1";
				//g.CellFormatting += (c, e) =>
				//  {
				//	  DataGridView grid = (c as DataGridView);
				//	  e.Value = (grid.DataSource as DataTable).Rows[e.RowIndex][e.ColumnIndex];

				//  };
				//g.CellValueChanged += (c, e) =>
				//  {
				//	  DataGridView grid = (c as DataGridView);
				//	  DataTable t = (grid.DataSource as DataTable);
				//	  t.Rows[e.RowIndex][e.ColumnIndex] = grid[e.ColumnIndex, e.RowIndex].Value;

				//  };

				cd.Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons);
				if (cd.ShowDialog() == DialogResult.OK)
				{
					cd.Save();
					//return items.Select(i=>i.Value).ToArray();
				}
				//else
				//	return items.Select(i => i.OriginalValue).ToArray();
			}
		}
	}
}
