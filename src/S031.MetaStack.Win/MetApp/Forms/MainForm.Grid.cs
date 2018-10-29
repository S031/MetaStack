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
		BindingSource _bs = new BindingSource();
		private WinFormItem SetGrid()
		{
			return new WinFormItem("MainGrid")
			{
				PresentationType = typeof(DBGridBase),
				CellAddress = new Pair<int>(0, 2),
				ControlTrigger = (item, c) =>
				{
					var grid = (c as DBGridBase);
					//grid.Name = "MainGrid";
					grid.SelectionStyle = DBGridSelectionStyle.CheckBox;
					grid.Dock = DockStyle.Fill;
					grid.Style = GridSyle.View;
					//grid.RowTemplate.Height = 25;
					grid.AllowAddObject = true;
					grid.AllowDelObject = true;
					grid.AllowEditObject = true;
					//grid.MultiSelect = false;
					grid.BackgroundColor = this.BackColor;
					grid.ObjectName = _objectName;
					grid.AddColumnsFromObjectSchema();
					if (_objectName.ToLower() == "dbo.dealvalue")
						_bs.DataSource = ClientGate.GetData(grid.ObjectName, "_filter", "Date = '2018-09-30'");
					else if (_objectName.ToLower() == "dbo.v_dealvalue")
						_bs.DataSource = ClientGate.GetData(grid.ObjectName,
							"@date", new DateTime(2018, 9, 30),
							"@DealType", "КредЮЛ_КК");
					else
						_bs.DataSource = ClientGate.GetData(grid.ObjectName);

					grid.DataSource = _bs;
					//grid.ObjectEditor = () => new Calculator(grid.ObjectName, GetItem("DOGrid").As<DBGridBase>().ReadObject());
					//grid.DataChanged += Grid_DataChanged;
					//grid.RowHeadersVisible = false;
					grid.ColumnHeadersVisible = true;
					//grid.Font = new Font(grid.Font.FontFamily, 10f);
					//grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					//grid.ObjectDel += _grid_ObjectDel;
					//grid.ObjectRead += _grid_ObjectRead;
					//grid.CellFormatting += _grid_CellFormatting;
					_grid = grid;
					SetMenuContext();
				}
			};
		}
	}
}
