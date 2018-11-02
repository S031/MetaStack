﻿using S031.MetaStack.Common;
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
		//Sort & filter support
		private class sortInfo
		{
			public int Index;
			public bool Ascend;
		}
		readonly List<sortInfo> sortedColumns = new List<sortInfo>();
		//Form
		private JMXSchema xc;
		private JMXSchema xcOriginal;
		private string _idColName;

		//private string shortcut;
		private object[] sqlParams;
		private DataTable dt;
		private static bool _firstStart = true;

		//Totals
		private struct totalInfo
		{
			public bool Check;
			public MacroType Type;
			public string Agregate;
			public decimal totalDec;
			public string totalString;
			public DateTime totalDate;
		}
		private totalInfo[] totals;
		private bool freezeFooter;

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
		public event EventHandler<ActionEventArgs> Filter;
		public event EventHandler<DBGridSortEventArgs> SortFirst;
		public event EventHandler<DBGridSortEventArgs> SortNext;

		protected virtual void OnFormNotRead(EventArgs e)
		{
			xc = null;
			EventHandler<EventArgs> te = SchemaNotRead;
			if (te != null) te(this, e);
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
		protected virtual void OnSortFirst(DBGridSortEventArgs e)
		{
			EventHandler<DBGridSortEventArgs> te = SortFirst;
			if (te != null)
				te(this, e);
			//!!!
			//else
			//	DoSortFirst(e.ColIndex);
		}
		protected virtual void OnSortNext(DBGridSortEventArgs e)
		{
			EventHandler<DBGridSortEventArgs> te = SortNext;
			if (te != null)
				te(this, e);
			//!!!
			//else
			//	DoSortNext(e.ColIndex);
		}
		public virtual void OnFilter(ActionEventArgs e)
		{
			EventHandler<ActionEventArgs> te = Filter;
			if (te != null)
				te(this, e);
			//!!!
			//else
			//	DoFilter();
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
			EventHandler eh = new EventHandler(mnuTotal_Click);
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
			//this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DBGrid_KeyDown);
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
				if (xc == null)
					return "";
				else
					return xc.ObjectName;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				bool read = false;
				JMXSchema s = null;
				try
				{
					s = ClientGate.GetObjectSchema(value);
					read = true;
				}
				catch (Connectors.TCPConnectorException e)
				{
					OnFormReadException(new SchemaExceptionEventArgs(e, xc));
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

		public JMXSchema Schema
		{
			get { return xc; }
			set
			{
				if (value == null) return;
				xc = value;

				//!!! Param dialog
				DBGridParam param = new DBGridParam(xc, this.ParentRow);
				if (param.ShowDialog( _firstStart? DBGridParamShowType.ShowAll: DBGridParamShowType.ShowSmart) == DialogResult.OK)
				{
					this.ObjectName = xc.ObjectName;
					_idColName = Schema.Attributes.FirstOrDefault(a => a.IsPK && GetMacroType(a.DataType) == MacroType.num)?.AttribName;
					sqlParams = param.GetQueryParameters();
					MakeFrame();
					xcOriginal = JMXSchema.Parse(xc.ToString());
					//!!!
					//mainform parentForm = (this.FindForm() as DBBrowser);
					//if (parentForm != null) parentForm.Caption = xc["Name"];
					OnFormComplete(new SchemaEventArgs(xc));
				}
				else
					OnFormNoComplete(new SchemaEventArgs(xc));
			}
		}

		private void MakeFrame()
		{
			freezeFooter = this.Footer;
			this.Footer = false;
			for (; this.Columns.Count > 0;) { this.Columns.Remove(this.Columns[0]); }
			this.AddColumnsFromObjectSchema();

			MakeRecordset();
		}

		private void MakeRecordset()
		{
			///!!! from schema
			int elapsedTime = 5000;
			if (elapsedTime < 1001)
			{
				try
				{
					GetRecordset();
				}
				catch (Connectors.TCPConnectorException e)
				{
					OnGetDataException(new SchemaExceptionEventArgs(e, xc));
					return;
				}
			}
			else
			{
				TimeWaitDialog tw = new TimeWaitDialog("", elapsedTime);
				tw.Show(this, GetRecordset);
				if (tw.Error != null)
				{
					OnGetDataException(new SchemaExceptionEventArgs(tw.Error, xc));
					return;
				}
			}
			var bs = new BindingSource();
			bs.DataSource = dt;
			this.DataSource = bs;

			if (freezeFooter) this.Footer = true;
			freezeFooter = false;
			MakeTotals();
			ShowTotals();
			this.Refresh();
		}

		private void GetRecordset()
		{
			string _objectName = xc.ObjectName;
			if (StartFilter.IsEmpty())
				dt = ClientGate.GetData(this.ObjectName, sqlParams);
			else
				dt = ClientGate.GetData(this.ObjectName, sqlParams.Concat(new object[] { "_filter", StartFilter }).ToArray());

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
		//public string GetStringForSelected(string fieldName)
		//{
		//	if (this.Rows.Count == 0)
		//		return string.Empty;
		//	else if (this.SelectedRows.Count <= 1)
		//	{
		//		DataRow row = GetRow(this.SelectedRows.Count == 0 ? this.CurrentCellAddress.Y : this.SelectedRows[0].Index);
		//		var att = xc.Attributes.FirstOrDefault(a => a.AttribName == fieldName);
		//		if (att == null)
		//			throw new ArgumentException("Неверно указан параметр имя поля в методе GetStringForSelected");
		//		if (row[fieldName] == DBNull.Value)
		//			return " Is Null";
		//		switch (GetMacroType(att.DataType))
		//		{
		//			case MacroType.num:
		//				return " = " + row[fieldName].ToString();
		//			case MacroType.log:
		//				return " = " + row[fieldName].ToString();
		//			case MacroType.date:
		//				return " = '" + string.Format("yyyyMMdd", row[fieldName]) + "'";
		//			default:
		//				return " = '" + row[fieldName].ToString() + "'";
		//		}
		//	}
		//	else
		//	{
		//		var att = xc.Attributes.FirstOrDefault(a => a.AttribName == fieldName);
		//		if (att == null)
		//			throw new ArgumentException("Неверно указан параметр имя поля в методе GetStringForSelected");
		//		MacroType tt = GetMacroType(att.DataType);
		//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		//		foreach (DataGridViewRow gridRow in this.SelectedRows)
		//		{
		//			DataRow row = GetRow(gridRow.Index);
		//			if (row[fieldName] != DBNull.Value)
		//			{
		//				switch (tt)
		//				{
		//					case MacroType.num:
		//						sb.Append("," + row[fieldName].ToString());
		//						break;
		//					case MacroType.log:
		//						sb.Append("," + row[fieldName].ToString());
		//						break;
		//					case MacroType.date:
		//						sb.Append(",'" + string.Format("yyyyMMdd", row[fieldName]) + "'");
		//						break;
		//					default:
		//						sb.Append(",'" + row[fieldName].ToString() + "'");
		//						break;
		//				}
		//			}
		//		}
		//		if (sb.Length == 0)
		//			return " Is Null";
		//		else
		//			return " In (" + sb.ToString().Substring(1) + ")";
		//	}
		//}
		#endregion Filter Sorting Selection


		#region Refresh
		public override void Reload()
		{
			this.Schema = xcOriginal;
		}
		public override void RefreshAll()
		{
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
		#endregion Refresh

		#region Totals
		private void MakeTotals()
		{
			int count = dt.Rows.Count;
			bool total = false;
			totals = new totalInfo[xc.Attributes.Count];
			for (int i = 0; i < xc.Attributes.Count; i++)
			{
				totals[i].Check = false;
				totals[i].Agregate = "";
				totals[i].Type = GetMacroType(xc.Attributes[i].DataType);
				totals[i].totalDec = 0;
				totals[i].totalString = "";
				totals[i].totalDate = DateTime.MinValue;

				string agr = xc.Attributes[i].Agregate.ToLower();
				if (TotalValid(totals[i].Type, agr))
				{
					totals[i].Check = true;
					totals[i].Agregate = agr;
					total = true;
					if (totals[i].Agregate == "rcnt")
						totals[i].totalDec = count;
					else if (totals[i].Agregate == "min")
						if (totals[i].Type == MacroType.date)
							totals[i].totalDate = DateTime.MaxValue;
						else if (totals[i].Type == MacroType.num)
							totals[i].totalDec = decimal.MaxValue;
						else if (totals[i].Type == MacroType.str)
							totals[i].totalString = ((char)12293).ToString();
				}
			}
			if (total)
			{
				//System.Threading.Tasks.Parallel.For(0, dt.Rows.Count, (i) =>
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					for (int j = 0; j < dt.Columns.Count; j++)
					{
						switch (totals[j].Agregate)
						{
							case "sum":
								totals[j].totalDec += Convert.ToDecimal(dt.Rows[i][j] ?? 0);
								break;
							case "ave":
								totals[j].totalDec += Convert.ToDecimal(dt.Rows[i][j] ?? 0) / count;
								break;
							case "vcnt":
								if (!vbo.IsEmpty(dt.Rows[i][j])) totals[j].totalDec++;
								break;
							case "max":
								if (!vbo.IsEmpty(dt.Rows[i][j]))
								{
									if (totals[j].Type == MacroType.num && Convert.ToDecimal(dt.Rows[i][j] ?? 0) > totals[j].totalDec)
										totals[j].totalDec = Convert.ToDecimal(dt.Rows[i][j] ?? 0);
									else if (totals[j].Type == MacroType.str && totals[j].totalString.CompareTo(dt.Rows[i][j].ToString()) < 0)
										totals[j].totalString = dt.Rows[i][j].ToString();
									else if (totals[j].Type == MacroType.date && (DateTime)dt.Rows[i][j] > totals[j].totalDate)
										totals[j].totalDate = (DateTime)dt.Rows[i][j];
								}
								break;
							case "min":
								if (!vbo.IsEmpty(dt.Rows[i][j]))
								{
									if (totals[j].Type == MacroType.num && Convert.ToDecimal(dt.Rows[i][j] ?? 0) < totals[j].totalDec)
										totals[j].totalDec = Convert.ToDecimal(dt.Rows[i][j] ?? 0);
									else if (totals[j].Type == MacroType.str && totals[j].totalString.CompareTo(dt.Rows[i][j].ToString()) > 0)
										totals[j].totalString = dt.Rows[i][j].ToString();
									else if (totals[j].Type == MacroType.date && (DateTime)dt.Rows[i][j] < totals[j].totalDate)
										totals[j].totalDate = (DateTime)dt.Rows[i][j];
								}
								break;
						}
					}
				}//);
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					if (totals[j].Agregate == "min")
					{
						if (totals[j].totalDec == decimal.MaxValue)
							totals[j].totalDec = 0;
						if (totals[j].totalDate == DateTime.MaxValue)
							totals[j].totalDate = DateTime.MinValue;
						if (totals[j].totalString == ((char)12293).ToString())
							totals[j].totalString = string.Empty;
					}


				}
			}
		}
		private void ShowTotals()
		{
			for (int i = 0; i < totals.Length; i++)
			{
				if (totals[i].Check)
				{
					if (totals[i].Agregate == "rcnt")
						this.FooterText(i, totals[i].totalDec.ToString("##0"));
					else if (totals[i].Type == MacroType.num)
						this.FooterText(i, totals[i].totalDec.ToString(this.Columns[i].DefaultCellStyle.Format));
					else if (totals[i].Type == MacroType.date)
						this.FooterText(i, totals[i].totalDate.ToString(vbo.DateFormat));
					else
						this.FooterText(i, totals[i].totalString);
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
		void mnuTotal_Click(object sender, EventArgs e)
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

			JMXAttribute att = xc.Attributes[col.Index];
			if (TotalValid(GetMacroType(att.DataType), totalFunc))
			{
				att.Agregate = totalFunc;
				xcOriginal.Attributes[col.Index].Agregate = totalFunc;
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
				if (string.IsNullOrEmpty(dt.DefaultView.RowFilter))
					this.GoTo(this.Rows.Count - 1);
				else
					this.GoTo(dt.DefaultView.Count - 1);
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
