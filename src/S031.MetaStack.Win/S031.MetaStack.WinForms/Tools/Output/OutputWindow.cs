using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public static class OutputWindow
	{
		private static readonly WinForm _cd;
		private static DataTable _dt;
		private static DataGridView _grid;
		private static readonly ConcurrentQueue<Tuple<DateTime, string, string>> _queue = new ConcurrentQueue<Tuple<DateTime, string, string>>(); 
		private static readonly Timer _timer;

		static OutputWindow()
		{
			_cd = new WinForm(WinFormStyle.Dialog) { Text = "Окно сообщений" };
			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			_cd.FormClosing += _cd_FormClosing;
			TableLayoutPanel tlpRows = _cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			tlpRows.Add(new WinFormItem($"LogView")
			{
				PresentationType = typeof(DataGridView),
				ControlTrigger = (cdi, ctrl) =>
				{
					DataGridView grid = (ctrl as DataGridView);
					grid.Dock = DockStyle.Fill;
					grid.AllowUserToAddRows = false;
					grid.AllowUserToDeleteRows = false;
					grid.AutoGenerateColumns = false;
					grid.VirtualMode = true;
					//grid.ScrollBars = ScrollBars.Both;
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
					col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					var b = new DataGridViewButtonColumn()
					{
						Name = "BtnView",
						HeaderText = "",
						Text = "***",
						Width = 30,
						UseColumnTextForButtonValue = true
					};
					grid.Columns.Add(b);
					grid.CellClick += Grid_CellClick;
					_grid = grid;
				}
			});
			_cd.Size = new Size(Convert.ToInt32(Screen.FromControl(_cd).WorkingArea.Width / vbo.GoldenRatio), Screen.FromControl(_cd).WorkingArea.Height / 3);
			_timer = new Timer { Interval = 1000 };
			_timer.Tick += (c, e) =>
			{
				int cnt = _queue.Count;
				for (; _queue.TryDequeue(out Tuple<DateTime, string, string> item);)
					_dt.Rows.Add(item.Item1, item.Item2, item.Item3);
				if (cnt > 0)
				{
					_grid.Refresh();
					Application.DoEvents();
				}
			};
			_cd.Disposed += (c, e) =>
			{
				_timer?.Dispose();
			};
			_grid.DataSource = GetDataSource();
		}

		private static void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex !=
				_grid.Columns["BtnView"].Index) return;
			WinForm.ShowStringDialog(_grid[2, e.RowIndex].Value.ToString(), "");
		}


		private static void GoToEnd() =>
			_grid.CurrentCell = _grid[_grid.CurrentCellAddress.X, _grid.RowCount - 1];


		private static void _cd_FormClosing(object sender, FormClosingEventArgs e)
		{
			_timer.Stop();
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
			_queue.Enqueue(new Tuple<DateTime, string, string>(DateTime.Now, level.ToString(), message));

		public static void Print(LogLevels level, string[] messages)
		{
			foreach (string item in messages)
				Print(level, item);
		}

		public static void Focus()
		{
			_cd.Focus();
		}

		public static void Clear()
		{
			_dt.Rows.Clear();
		}

		public static void Show()
		{
			_cd.Visible = true;
			_timer.Start();
		}

		public static void Exit()
		{
			_cd?.Close();
			_cd.Dispose();
		}
	}
}
