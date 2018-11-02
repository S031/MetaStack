using S031.MetaStack.Common;
using S031.MetaStack.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetApp
{
	internal sealed partial class MainForm : WinForm
	{
		private WinFormItem SetGrid()
		{
			return new WinFormItem("MainGrid")
			{
				PresentationType = typeof(DBGrid),
				CellAddress = new Pair<int>(0, 2),
				ControlTrigger = (item, c) =>
				{
					var grid = (c as DBGrid);
					//grid.Name = "MainGrid";
					///!!!
					grid.Dock = DockStyle.Fill;
					grid.Style = GridSyle.View;
					grid.Footer = true;
					//grid.RowTemplate.Height = 25;
					grid.AllowAddObject = true;
					grid.AllowDelObject = true;
					grid.AllowEditObject = true;
					//grid.GridColor = this.BackColor;
					//grid.BackgroundColor = this.BackColor;
					//grid.RowHeadersDefaultCellStyle.BackColor = this.BackColor;
					//grid.MultiSelect = false;
					//grid.ObjectEditor = () => new Calculator(grid.ObjectName, GetItem("DOGrid").As<DBGridBase>().ReadObject());
					//grid.DataChanged += Grid_DataChanged;
					//grid.RowHeadersVisible = false;
					grid.ColumnHeadersVisible = true;
					//grid.Font = new Font(grid.Font.FontFamily, 10f);
					//grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					//grid.ObjectDel += _grid_ObjectDel;
					//grid.ObjectRead += _grid_ObjectRead;
					//grid.CellFormatting += _grid_CellFormatting;
					grid.StartFilter = _formOptions.StartFilter;
					grid.SchemaName = _objectName;
					grid.SetSpeedSearch((ToolStripTextBox)GetItem("ToolBar").As<ToolStrip>().Items["txtFind"]);
					_grid = grid;
					SetMenuContext();
				}
			};
		}
	}
}
