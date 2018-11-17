using System;
using System.Drawing;
using System.Windows.Forms;
using S031.MetaStack.Common.Logging;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Data;

namespace S031.MetaStack.WinForms
{
	public static class OutputWindow
	{
		private static WinForm _cd;
		private static DataTable _dt = null;
		private static DataGridView _grid = null;


		static OutputWindow()
		{
			_cd = new WinForm(WinFormStyle.Dialog);
			_cd.Size = new Size(Screen.FromControl(_cd).WorkingArea.Width / 2, Screen.FromControl(_cd).WorkingArea.Height / 3); 
			_cd.Text = "Окно сообщений";
			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			_cd.FormClosing += _cd_FormClosing;
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
					int i = grid.Columns.Add("MessageTime", "Время");
					var col = grid.Columns[i];
					col.DataPropertyName = "MessageTime";
					i = grid.Columns.Add("MessageSource", "Статус");
					grid.Columns[i].DataPropertyName = "MessageSource";
					i = grid.Columns.Add("Message", "Описание события");
					grid.Columns[i].DataPropertyName = "Message";
					grid.Columns[i].Width = _cd.Width - grid.Columns[0].Width - grid.Columns[1].Width - 60;
					grid.DataSource = GetDataSource();
					_dt.RowChanged += GoToEnd;
					_grid = grid;
				}
			});
		}

		private static void GoToEnd(object sender, DataRowChangeEventArgs e) =>
			_grid.CurrentCell = _grid[_grid.CurrentCellAddress.X, _grid.RowCount - 1];

		private static void _cd_FormClosing(object sender, FormClosingEventArgs e)
		{
			Clear();
			_cd.Visible = false;
			e.Cancel = true;
		}

		private static void _cd_Resize(object sender, EventArgs e)
		{
			if (_cd.Visible && _cd.WindowState == FormWindowState.Normal)
			{
				//FormSettings.Default.OutputWindow_Size = _cd.Size;
				//FormSettings.Default.Save();
			}
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

		public static void Print(LogLevels level, string message)
		{
			DataRow dr = _dt.NewRow();
			dr["MessageTime"] = DateTime.Now;
			dr["MessageSource"] = FileLog.Messages[level];
			dr["Message"] = message;
			_dt.Rows.Add(dr);
			//_dt.Rows.Add(DateTime.Now, FileLog.Messages[level], message);
		}

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
