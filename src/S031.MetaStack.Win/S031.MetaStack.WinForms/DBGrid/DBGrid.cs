using S031.MetaStack.Common;
using S031.MetaStack.WinForms.ORM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public class DBGridSortEventArgs : EventArgs
	{
		public int ColIndex { get; private set; }
		public DBGridSortEventArgs(int sortColIndex) => ColIndex = sortColIndex;
	}

	public class DBGrid : DBGridBase
	{
		#region Private declarations
		private const string UserNameField = "UserNameField";
		
		//Form
		private JMXSchema _xc;
		private JMXSchema _xcOriginal;
		private string _idColName;

		//private string shortcut;
		private object[] _sqlParams;
		private DataTable _dt;
		private string _connectioName;
		private static bool _firstStart = true;

		//Totals
		private struct TotalInfo
		{
			public bool Check;
			public MacroType Type;
			public string Agregate;
			public decimal totalDec;
			public string totalString;
			public DateTime totalDate;
		}
		private TotalInfo[] _totals;
		private bool _freezeFooter;

		//Clipboard Support
		private static bool delObj;
		#endregion Private declarations

		#region Public Events
		public event EventHandler<EventArgs>SchemaNotRead;
		public event EventHandler<SchemaEventArgs> SchemaRead;
		public event EventHandler<SchemaEventArgs> SchemaComplete;
		public event EventHandler<SchemaEventArgs> SchemaNoComplete;
		public event EventHandler<SchemaExceptionEventArgs> SchemaReadException;
		public event EventHandler<SchemaExceptionEventArgs> GetDataException;

		protected virtual void OnFormNotRead(EventArgs e)
		{
			_xc = null;
			SchemaNotRead?.Invoke(this, e);
		}
		protected virtual void OnFormRead(SchemaEventArgs e)
		{
			EventHandler<SchemaEventArgs> te = SchemaRead;
			if (te != null)
				te(this, e);
			else
			{
				//this.Schema = ClientGate.GetObjectSchema(e.ObjSchema.ObjectName);
				this.Schema = e.ObjSchema;
				_firstStart = false;
			}
		}
		protected virtual void OnFormComplete(SchemaEventArgs e)
		{
			SchemaComplete?.Invoke(this, e);
		}
		protected virtual void OnFormNoComplete(SchemaEventArgs e)
		{
			SchemaNoComplete?.Invoke(this, e);
		}
		protected virtual void OnFormReadException(SchemaExceptionEventArgs e)
		{
			EventHandler<SchemaExceptionEventArgs> te = SchemaReadException;
			if (te != null)
				te(this, e);
			else
				MessageBox.Show(e.Exception.Message, "Ошибка открытия формы", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		protected virtual void OnGetDataException(SchemaExceptionEventArgs e)
		{
			EventHandler<SchemaExceptionEventArgs> te = GetDataException;
			if (te != null)
				te(this, e);
			else
				MessageBox.Show(e.Exception.Message, "Ошибка выполнения запроса", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		#endregion Public Events

		#region Constructor
		public DBGrid()
		{
			this.SelectionStyle = DBGridSelectionStyle.CheckBox;
			//
			// mnuTotal
			//
			ContextMenuStrip mnuTotal = new ContextMenuStrip();
			EventHandler eh = new EventHandler(MnuTotal_Click);
			mnuTotal.Items.Add(new ToolStripMenuItem("Сумма", null, eh) { Name = "mnuTotalSum" });
			mnuTotal.Items.Add(new ToolStripMenuItem("Количество", null, eh) { Name = "mnuTotalCount" });
			mnuTotal.Items.Add(new ToolStripMenuItem("Среднее", null, eh) { Name = "mnuTotalAverage" });
			mnuTotal.Items.Add(new ToolStripMenuItem("Максимум", null, eh) { Name = "mnuTotalMax" });
			mnuTotal.Items.Add(new ToolStripMenuItem("Минимум", null, eh) { Name = "mnuTotalMin" });
			mnuTotal.Items.Add(new ToolStripSeparator());
			mnuTotal.Items.Add(new ToolStripMenuItem("Копировать", null, eh) { Name = "mnuTotalCopy" });
			FooterContextMenu = mnuTotal;
			//FontManager.Adop(mnuTotal);
			// 
			// DBGrid
			// 
			this.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DBGrid_ColumnHeaderMouseClick);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DBGrid_KeyDown);
			//this.ObjectRead += new EventHandler<ActionEventArgs>(DBGrid_ObjectRead);
			//this.ObjectReadNew += new EventHandler<ActionEventArgs>(DBGrid_ObjectReadNew);
			//this.ObjectCopy += new EventHandler<ActionEventArgs>(DBGrid_ObjectCopy);
			//this.ObjectCut += new EventHandler<ActionEventArgs>(DBGrid_ObjectCut);
			//this.ObjectPaste += new EventHandler<ActionEventArgs>(DBGrid_ObjectPaste);
		}
		#endregion Constructor

		#region Schema Support
		public string SchemaName
		{
			get
			{
				if (_xc == null)
					return "";
				else
					return _xc.ObjectName;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				bool read = false;
				JMXSchema s = null;
				try
				{
					s = ClientGate.GetObjectSchema(ParseObjectName(value));
					read = true;
				}
				catch (Connectors.TCPConnectorException e)
				{
					OnFormReadException(new SchemaExceptionEventArgs(e, _xc));
					return;
				}
				if (read)
				{
					OnFormRead(new SchemaEventArgs(s));
				}
				else
					OnFormNotRead(new EventArgs());
			}
		}
		private string ParseObjectName(string value)
		{
			string[] name = (".." + value).Split('.');
			int i = name.Length - 1;
			string objectName = name[i];
			string owner = name[i - 1];
			string connectionName = name[i - 2];

			if (!connectionName.IsEmpty())
				_connectioName = connectionName;
			if (!owner.IsEmpty())
				objectName = owner + '.' + objectName;
			return objectName;
		}
		public JMXSchema Schema
		{
			get { return _xc; }
			set
			{
				LoadComplete = false;
				if (value == null) return;
				_xc = value;

				DBGridParam param = new DBGridParam(_xc, this);
				if (param.ShowDialog( _firstStart? DBGridParamShowType.ShowAll: DBGridParamShowType.ShowSmart) == DialogResult.OK)
				{
					this.ObjectName = _xc.ObjectName;
					_idColName = Schema.Attributes.FirstOrDefault(a => a.IsPK && GetMacroType(a.DataType) == MacroType.num)?.AttribName;
					_sqlParams = param.GetQueryParameters();
					MakeFrame();
					_xcOriginal = JMXSchema.Parse(_xc.ToString());
					var parentForm = this.FindForm();
					parentForm.Text = _xc.Name;
					OnFormComplete(new SchemaEventArgs(_xc));
				}
				else
					OnFormNoComplete(new SchemaEventArgs(_xc));
				LoadComplete = true;
			}
		}

		private void MakeFrame()
		{
			_freezeFooter = this.Footer;
			this.Footer = false;
			for (; this.Columns.Count > 0;) { this.Columns.Remove(this.Columns[0]); }
			this.AddColumnsFromObjectSchema();

			MakeRecordset();
		}

		private void MakeRecordset()
		{
			var fs = new FormOpenTimeStatistics();
			int elapsedTime = fs.GetTime(SchemaName, 5000);
			DateTime startTime = DateTime.Now;
			if (elapsedTime < 1001)
			{
				try
				{
					GetRecordset();
				}
				catch (Connectors.TCPConnectorException e)
				{
					OnGetDataException(new SchemaExceptionEventArgs(e, _xc));
					return;
				}
			}
			else
			{
				TimeWaitDialog tw = new TimeWaitDialog("", elapsedTime);
				tw.Show(this, GetRecordset);
				if (tw.Error != null)
				{
					OnGetDataException(new SchemaExceptionEventArgs(tw.Error, _xc));
					return;
				}
			}
			int factTime = Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
			if (factTime > elapsedTime)
				fs.SetTime(SchemaName, factTime);
			var bs = new BindingSource
			{
				DataSource = _dt,
			};
			bs.ListChanged += (c, e) => { MakeTotals(); ShowTotals(); };
			this.DataSource = bs;

			if (_freezeFooter) this.Footer = true;
			_freezeFooter = false;
			MakeTotals();
			ShowTotals();
			this.Refresh();
		}

		private void GetRecordset()
		{
			string _objectName = _xc.ObjectName;
			var parameters = _sqlParams.AsEnumerable();

			if (!StartFilter.IsEmpty())
				parameters = parameters.Concat(new object[] { "_filter", StartFilter });
			if (!_connectioName.IsEmpty())
				parameters = parameters.Concat(new object[] { "_connectionName", _connectioName });

			_dt = ClientGate.GetData(this.ObjectName, parameters.ToArray());

		}

		public DataRow ParentRow { get; set; }
		#endregion Schema Support

		#region Filter Sorting Selection
		public string IdColName => _idColName;

		public string StartFilter { get; set; }

		public string GetStringForSelected()
		{
			return this.GetStringForSelected(_idColName);
		}
		#endregion Filter Sorting Selection


		#region Refresh
		public override void Reload()
		{
			if (LoadComplete)
				this.Schema = _xcOriginal;
		}
		public override void RefreshAll()
		{
			if (LoadComplete)
				MakeRecordset();
		}
		public void RefreshAll(int handle)
		{
			int offset = (this.CurrentCellAddress.Y - this.FirstDisplayedScrollingRowIndex);

			int i = this.CurrentCellAddress.X;
			if (i == -1) i = 0;
			MakeRecordset();
			if (!_idColName.IsEmpty())
				this.Find(dr => dr[_idColName].Equals(handle));
			int rowNumber = this.CurrentCellAddress.Y;
			if (rowNumber > 1)
			{
				this.FirstDisplayedScrollingRowIndex = rowNumber - offset >= 0 ? rowNumber - offset : rowNumber;
				this.CurrentCell = this.CurrentRow.Cells[i];
			}
		}
		public override void RefreshAll(object bookmark)
		{
			this.RefreshAll((int)bookmark);
		}
		public bool LoadComplete { get; protected set; }
		#endregion Refresh

		#region Totals
		private void MakeTotals()
		{
			var bs = GetBindingSource();
			if (bs == null)
				return;
			var list = bs.List;

			int count = list.Count;
			bool total = false;
			_totals = new TotalInfo[_xc.Attributes.Count];
			for (int i = 0; i < _xc.Attributes.Count; i++)
			{
				_totals[i].Check = false;
				_totals[i].Agregate = "";
				_totals[i].Type = GetMacroType(_xc.Attributes[i].DataType);
				_totals[i].totalDec = 0;
				_totals[i].totalString = "";
				_totals[i].totalDate = DateTime.MinValue;

				string agr = _xc.Attributes[i].Agregate.ToLower();
				if (TotalValid(_totals[i].Type, agr))
				{
					_totals[i].Check = true;
					_totals[i].Agregate = agr;
					total = true;
					if (_totals[i].Agregate == "rcnt")
						_totals[i].totalDec = count;
					else if (_totals[i].Agregate == "min")
						if (_totals[i].Type == MacroType.date)
							_totals[i].totalDate = DateTime.MaxValue;
						else if (_totals[i].Type == MacroType.num)
							_totals[i].totalDec = decimal.MaxValue;
						else if (_totals[i].Type == MacroType.str)
							_totals[i].totalString = ((char)12293).ToString();
				}
			}
			if (total)
			{
				//Parallel.For(0, count, (i) =>
				for (int i = 0; i < count; i++)
				{
					var row = (list[i] as DataRowView);
					for (int j = 0; j < _dt.Columns.Count; j++)
					{
						object value = row[j];
						if (!vbo.IsEmpty(value))
						{
							switch (_totals[j].Agregate)
							{
								case "sum":
									_totals[j].totalDec += Convert.ToDecimal(value);
									break;
								case "ave":
									_totals[j].totalDec += Convert.ToDecimal(value) / count;
									break;
								case "vcnt":
									if (!vbo.IsEmpty(value)) _totals[j].totalDec++;
									break;
								case "max":
										if (_totals[j].Type == MacroType.num && Convert.ToDecimal(value) > _totals[j].totalDec)
											_totals[j].totalDec = Convert.ToDecimal(value);
										else if (_totals[j].Type == MacroType.str && _totals[j].totalString.CompareTo(value.ToString()) < 0)
											_totals[j].totalString = value.ToString();
										else if (_totals[j].Type == MacroType.date && (DateTime)value > _totals[j].totalDate)
											_totals[j].totalDate = (DateTime)value;
									break;
								case "min":
										if (_totals[j].Type == MacroType.num && Convert.ToDecimal(value) < _totals[j].totalDec)
											_totals[j].totalDec = Convert.ToDecimal(value);
										else if (_totals[j].Type == MacroType.str && _totals[j].totalString.CompareTo(value.ToString()) > 0)
											_totals[j].totalString = value.ToString();
										else if (_totals[j].Type == MacroType.date && (DateTime)value < _totals[j].totalDate)
											_totals[j].totalDate = (DateTime)value;
									break;
							}
						}
					}
				}//);
				for (int j = 0; j < this.Columns.Count; j++)
				{
					if (_totals[j].Agregate == "min")
					{
						if (_totals[j].totalDec == decimal.MaxValue)
							_totals[j].totalDec = 0;
						if (_totals[j].totalDate == DateTime.MaxValue)
							_totals[j].totalDate = DateTime.MinValue;
						if (_totals[j].totalString == ((char)12293).ToString())
							_totals[j].totalString = string.Empty;
					}


				}
			}
		}
		private void ShowTotals()
		{
			if (_totals == null)
				return;
			for (int i = 0; i < _totals.Length; i++)
			{
				if (_totals[i].Check)
				{
					if (_totals[i].Agregate == "rcnt")
						this.FooterText(i, _totals[i].totalDec.ToString("##0"));
					else if (_totals[i].Type == MacroType.num)
						this.FooterText(i, _totals[i].totalDec.ToString(this.Columns[i].DefaultCellStyle.Format));
					else if (_totals[i].Type == MacroType.date)
						this.FooterText(i, _totals[i].totalDate.ToString(vbo.DateFormat));
					else
						this.FooterText(i, _totals[i].totalString);
				}
			}
		}
		private static bool TotalValid(MacroType tt, string totalFunc)
		{
			if (string.IsNullOrEmpty(totalFunc) || totalFunc == "none")
				return false;
			else if (tt == MacroType.log && totalFunc != "rcnt")
				return false;
			else if ((tt == MacroType.date || tt == MacroType.str) && (totalFunc == "sum" || totalFunc == "ave"))
				return false;
			return true;
		}
		#endregion Totals

		#region Events  implements & Overrides
		//Disable process for key minus in base classes
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.OemMinus)
			{
				//this.Reload();
				FilterClear();
				return;
			}
			base.OnKeyDown(e);
		}
		private void MnuTotal_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = (sender as ToolStripMenuItem);
			ContextMenuStrip ms = (mi.Owner as ContextMenuStrip);
			DataGridViewColumn col = (DataGridViewColumn)((ms.SourceControl as Label).Tag);
			string totalFunc = string.Empty;
			switch (mi.Name)
			{
				case "mnuTotalSum":
					totalFunc = "sum";
					break;
				case "mnuTotalAverage":
					totalFunc = "ave";
					break;
				case "mnuTotalCount":
					totalFunc = "rcnt";
					break;
				case "mnuTotalMin":
					totalFunc = "min";
					break;
				case "mnuTotalMax":
					totalFunc = "max";
					break;
				case "mnuTotalCopy":
					string fText = this.FooterText(col.Index).Replace(
						System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator, "");
					if (!string.IsNullOrEmpty(fText))
						Clipboard.SetText(fText);
					return;
			}

			JMXAttribute att = _xc.Attributes[col.Index];
			if (TotalValid(GetMacroType(att.DataType), totalFunc))
			{
				att.Agregate = totalFunc;
				_xcOriginal.Attributes[col.Index].Agregate = totalFunc;
				MakeTotals();
				ShowTotals();
			}
		}
		private void DBGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert))
			{
				OnObjectCopy(new ActionEventArgs(this.ReadObject(false)));
				e.SuppressKeyPress = true;
			}
			else if ((e.Control && e.KeyCode == Keys.X) || (e.Shift && e.KeyCode == Keys.Delete))
			{
				OnObjectCut(new ActionEventArgs(this.ReadObject(false)));
				e.SuppressKeyPress = true;
			}
			else if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
			{
				OnObjectPaste(new ActionEventArgs(null));
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Oemplus)
				OnFilter(new ActionEventArgs(null));
			else if (e.Control || e.Shift || e.Alt) { }
			else if (e.KeyCode == Keys.Home)
			{
				this.GoTo(0);
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.End)
			{
				this.GoTo(this.Rows.Count - 1);
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.F1)
			{
				//MessageBox.Show(this.GetStringForSelected("Handle"));
			}
		}
		private void DBGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (Control.ModifierKeys == Keys.Control && e.Button == MouseButtons.Left)
			{
				OnSortNext(new DBGridSortEventArgs(e.ColumnIndex));
			}
			else if (e.Button == MouseButtons.Left)
			{
				OnSortFirst(new DBGridSortEventArgs(e.ColumnIndex));
			}
		}
		#endregion Events  implements
	}
}
