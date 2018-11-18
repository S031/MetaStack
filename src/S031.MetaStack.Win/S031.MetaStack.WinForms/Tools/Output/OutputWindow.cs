using System;
using System.Drawing;
using System.Windows.Forms;
using S031.MetaStack.Common.Logging;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Data;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public static class OutputWindow
	{
		private static WinForm _cd;
		private static DataTable _dt = null;
		private static DataGridView _grid = null;


		static OutputWindow()
		{
			_cd = new WinForm(WinFormStyle.Dialog) { Text = "Окно сообщений" };
			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			_cd.FormClosing += _cd_FormClosing;
			_cd.Resize += _cd_Resize;
			TableLayoutPanel tlpRows = _cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			tlpRows.Add(new WinFormItem($"LogView")
			{
				PresentationType = typeof(DataGridView),
				ControlTrigger = (cdi, ctrl) =>
				{
					DataGridView grid = (ctrl as DataGridView);
					grid.AllowUserToAddRows = false;
					grid.AllowUserToDeleteRows = false;
					grid.ScrollBars = ScrollBars.None;
					grid.RowHeadersWidth = 30;
					int i = grid.Columns.Add("MessageTime", "Время");
					var col = grid.Columns[i];
					col.DataPropertyName = "MessageTime";
					col.DefaultCellStyle.Format = vbo.FullDateFormat;
					col.Width = 130;
					i = grid.Columns.Add("MessageSource", "Статус");
					grid.Columns[i].DataPropertyName = "MessageSource";
					i = grid.Columns.Add("Message", "Описание события");
					col = grid.Columns[i];
					col.DataPropertyName = "Message";
					col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
					var b = new DataGridViewButtonColumn()
					{
						Name = "BtnView",
						HeaderText = "",
						Text = "***",
						Width = 30,
						UseColumnTextForButtonValue = true
					};
					grid.Columns.Add(b);
					grid.DataSource = GetDataSource();
					grid.CellClick += Grid_CellClick;
					_dt.RowChanged += GoToEnd;
					_grid = grid;
				}
			});
			_cd.Size = new Size(Convert.ToInt32(Screen.FromControl(_cd).WorkingArea.Width / vbo.GoldenRatio), Screen.FromControl(_cd).WorkingArea.Height / 3); 
		}

		private static void _cd_Resize(object sender, EventArgs e)
		{
			_grid.Columns["Message"].Width = _cd.Width - _grid.Columns["MessageTime"].Width - _grid.Columns["MessageSource"].Width - 90;
		}

		private static void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex !=
				_grid.Columns["BtnView"].Index) return;
			WinForm.ShowStringDialog(_grid[3, e.RowIndex].Value.ToString(), "");
		}

		private static void GoToEnd(object sender, DataRowChangeEventArgs e) =>
			_grid.CurrentCell = _grid[_grid.CurrentCellAddress.X, _grid.RowCount - 1];

		private static void _cd_FormClosing(object sender, FormClosingEventArgs e)
		{
			Clear();
			_cd.Visible = false;
			e.Cancel = true;
		}

		public static DataTable GetDataSource()
		{
			if (_dt == null)
			{
				_dt = new DataTable("PipeReadData");
				_dt.Columns.Add("MessageTime", typeof(DateTime));
				_dt.Columns.Add(new DataColumn("MessageSource", typeof(string)) { MaxLength = 16 });
				_dt.Columns.Add(new DataColumn("Message", typeof(string)) { MaxLength = 2048*1024 });
			}
			return _dt;
		}

		public static void Print(string message) => Print(LogLevels.None, message);

		public static void Print(LogLevels level, string message)=>
			_dt.Rows.Add(DateTime.Now, FileLog.Messages[level], message);

		public static void Print(LogLevels level, string[] messages)
		{
			_dt.RowChanged -= GoToEnd;
			foreach (string item in messages)
				Print(level, item);
			GoToEnd(_grid, null);
			_dt.RowChanged += GoToEnd;
		}

		public static void Focus()
		{
			_cd.Focus();
		}

		public static void Clear()
		{
			_dt.Rows.Clear();
		}

		public static void Show() => _cd.Visible = true;
	}
}
