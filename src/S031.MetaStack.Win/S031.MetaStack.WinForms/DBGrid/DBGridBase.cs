using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using S031.MetaStack.Common;
using Newtonsoft.Json.Linq;
using S031.MetaStack.WinForms.Data;
using S031.MetaStack.WinForms.Json;
using S031.MetaStack.WinForms.ORM;

namespace S031.MetaStack.WinForms
{

	public enum GridSyle
	{
		View = 0,
		Edit = 1
	}

	public enum DBGridSelectionStyle
	{
		Normal = 0,
		CheckBox = 1
	}

	public enum MacroType
	{
		num = 0,
		date = 1,
		str = 2,
		log = 3,
		blob = 4
	}

	public class DBGridBase : DataGridView, IObjectHost
	{
		public const string TransportObject = "Qualifier";
		//public const string DetailExtendedSection = "__detailExtended.031";
		//public const string DetailObjectFlagProperty = "__detailObject.031";
		//public const string DetailObjectNameProperty = "__objectName.031";
		//public const string DetailObjectRowIndexProperty = "__rowIndex.031";

		#region Private declarations
		//Data Source
		private DataTable dt;

		//GridStyle, RaiseCell
		private GridSyle gs;
		private bool raiseCell;

		//Selection 
		DBGridSelectionStyle _selectionStyle = DBGridSelectionStyle.Normal;
		List<DataGridViewRow> checkedRows = new List<DataGridViewRow>();

		//Footer
		private bool lastRowPainted;
		private const string footerLabelName = "lblFooter";
		private bool footer;
		private Label lblFooter;
		private Dictionary<DataGridViewColumn, Label> totalLabels = new Dictionary<DataGridViewColumn, Label>();
		private ContextMenuStrip mnuTotal;
		protected Panel footerPanel { get; private set; }

		//Search support
		private object lastSearch;
		private string lastCondition = string.Empty;
		private int oldCol = -1;
		private static List<string> searches = new List<string>();
		private GridSpeedSearch _sss;
		#endregion Private declarations

		#region Constructor
		public DBGridBase()
		{
			this.SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);
			InitializeComponent();
			this.AllowAddObject = true;
			this.AllowDelObject = true;
			this.AllowEditObject = true;
			this.StandardTab = true;
		}

		private void InitializeComponent()
		{
			this.footerPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// footerPanel
			// 
			this.footerPanel.BackColor = System.Drawing.SystemColors.Control;
			this.footerPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.footerPanel.Location = new System.Drawing.Point(0, 0);
			this.footerPanel.MinimumSize = new System.Drawing.Size(0, 18);
			this.footerPanel.Name = "footerPanel";
			this.footerPanel.Size = new System.Drawing.Size(0, this.RowTemplate.Height);
			this.footerPanel.TabIndex = 0;
			this.footerPanel.BorderStyle = BorderStyle.None;

			this.Controls.Add(this.footerPanel);
			//
			// lblFooter
			//
			lblFooter = new Label();
			lblFooter.Name = footerLabelName;
			lblFooter.TextAlign = ContentAlignment.MiddleLeft;
			lblFooter.Dock = DockStyle.Left;
			this.footerPanel.Controls.Add(lblFooter);
			// 
			// DBGridBase
			// 
			this.AutoGenerateColumns = false;
			this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; //.ColumnHeadersHeight = 18;
			this.RowHeadersWidth = 18;
			this.RowTemplate.Height = (int)(this.Font.GetHeight() * 1.5);
			this.VirtualMode = true;
			this.KeyDown += new KeyEventHandler(DBGridBase_KeyDown);
			this.CurrentCellChanged += new EventHandler(DBGridBase_CurrentCellChanged);
			this.CellFormatting += new DataGridViewCellFormattingEventHandler((obj, e) =>
			{
				DataGridViewColumn col = this.Columns[e.ColumnIndex];
				if (col.DefaultCellStyle.FormatProvider is AccntFormat)
					e.Value = string.Format(col.DefaultCellStyle.FormatProvider, AccntFormat.ACCNTFORMAT, e.Value);
				DataGridViewRow r = this.Rows[e.RowIndex];
				if (checkedRows.Contains(r))
				{
					e.CellStyle.BackColor = this.RowTemplate.DefaultCellStyle.SelectionBackColor;
					e.CellStyle.ForeColor = this.RowTemplate.DefaultCellStyle.SelectionForeColor;
				}

			});
			this.RowHeaderMouseClick += new DataGridViewCellMouseEventHandler((obj, e) =>
			{
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					CheckRows(this.Rows[e.RowIndex]);
				}
				else
					CheckRow(this.Rows[e.RowIndex]);
			});
			this.CellMouseClick += new DataGridViewCellMouseEventHandler((obj, e) =>
			{
				if (e.RowIndex == -1 && e.ColumnIndex == -1)
				{
					if (_selectionStyle == DBGridSelectionStyle.Normal)
						this.SelectAll();
					//else if (checkedRows.Count == this.Rows.Count)
					else if (checkedRows.Count > 0)
					{
						checkedRows.Clear();
						this.Refresh();
					}
					else
						this.SelectAll();

				}
			});
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.DataSourceChanged += new EventHandler(DBGridBase_DataSourceChanged);
			this.ResumeLayout(false);
		}

		void CheckRow(DataGridViewRow r)
		{
			checkRow(r);
			this.Refresh();
		}

		void checkRow(DataGridViewRow r)
		{
			if (r.Index > -1 && _selectionStyle == DBGridSelectionStyle.CheckBox)
			{
				if (checkedRows.Contains(r))
					checkedRows.Remove(r);
				else
					checkedRows.Add(r);
			}
		}

		void CheckRows(DataGridViewRow r)
		{
			if (checkedRows.Count == 0)
				CheckRow(this.Rows[r.Index]);
			else
			{
				int idx = r.Index;
				int idxChecked = -1;

				var rows = checkedRows.Where((row) => row.Index < r.Index);
				if (rows.Any())
					idxChecked = rows.Max((row) => row.Index);
				else
				{
					rows = checkedRows.Where((row) => row.Index > r.Index);
					if (rows.Any())
						idxChecked = rows.Min((row) => row.Index);
					else
						idxChecked = idx;
				}


				if (idxChecked < idx)
				{
					for (int i = idx; i > idxChecked; i--)
					{
						if (!checkedRows.Contains(this.Rows[i]))
							checkRow(this.Rows[i]);
					}
				}
				else
				{
					for (int i = idx; i < idxChecked; i++)
					{
						if (!checkedRows.Contains(this.Rows[i]))
							checkRow(this.Rows[i]);
					}
				}
				this.Refresh();
			}
		}

		#endregion Constructor

		#region Public Events
		public event EventHandler<ActionEventArgs> FindFirst;
		public event EventHandler<ActionEventArgs> FindNext;
		public event EventHandler<ActionEventArgs> ObjectRead;
		public event EventHandler<ActionEventArgs> ObjectReadNew;
		public event EventHandler<ActionEventArgs> ObjectAdd;
		public event EventHandler<ActionEventArgs> ObjectEdit;
		public virtual event EventHandler<ActionEventArgs> ObjectDel;
		public event EventHandler<ActionEventArgs> ObjectCut;
		public event EventHandler<ActionEventArgs> ObjectCopy;
		public event EventHandler<ActionEventArgs> ObjectPaste;
		public event EventHandler<ActionEventArgs> DataChanged;
		public event EventHandler<ActionEventArgs> DataDeleted;

		public virtual void OnFindFirst(ActionEventArgs e)
		{
			EventHandler<ActionEventArgs> te = FindFirst;
			if (te != null)
				te(this, e);
			else
				DoFindFirst();
		}
		public virtual void OnFindNext(ActionEventArgs e)
		{
			if (lastSearch == null)
				OnFindFirst(e);
			else
			{
				EventHandler<ActionEventArgs> te = FindNext;
				if (te != null)
					te(this, e);
				else if (this.Rows.Count > 0)
					DoConditionFind(this.CurrentCellAddress.Y + 1);
			}
		}
		public virtual void OnObjectRead(ActionEventArgs e)
		{
			if (!ReadOnly)
			{
				EventHandler<ActionEventArgs> te = ObjectRead;
				if (te != null)
					te(this, e);
				else
					e.ObjSource = DoObjectRead(this.Rows.Count == 0);
			}
		}
		public virtual void OnObjectReadNew(ActionEventArgs e)
		{
			if (!ReadOnly)
			{
				EventHandler<ActionEventArgs> te = ObjectReadNew;
				if (te != null)
					te(this, e);
				else
					e.ObjSource = DoObjectRead(true);
			}
		}
		public virtual void OnObjectAdd(ActionEventArgs e)
		{
			if (AllowAddObject)
			{
				EventHandler<ActionEventArgs> te = ObjectAdd;
				if (te != null)
					te(this, e);
				else
					DoObjectEditAdd(e.ObjSource, true, e.ActionID);

			}
		}
		public virtual void OnObjectEdit(ActionEventArgs e)
		{
			if (AllowEditObject)
			{
				EventHandler<ActionEventArgs> te = ObjectEdit;
				if (te != null)
					te(this, e);
				else
					DoObjectEditAdd(e.ObjSource, (this.Rows.Count == 0), e.ActionID);
			}
		}
		public virtual void OnObjectDel(ActionEventArgs e)
		{
			if (AllowDelObject)
			{
				EventHandler<ActionEventArgs> te = ObjectDel;
				if (te != null)
					te(this, e);
				else
					DoObjectDel(e.ObjSource);
			}
		}
		public virtual void OnObjectCut(ActionEventArgs e)
		{
			if (!ReadOnly)
			{
				EventHandler<ActionEventArgs> te = ObjectCut;
				if (te != null) te(this, e);
			}
		}
		public virtual void OnObjectCopy(ActionEventArgs e)
		{
			if (!ReadOnly)
			{
				EventHandler<ActionEventArgs> te = ObjectCopy;
				if (te != null) te(this, e);
			}
		}
		public virtual void OnObjectPaste(ActionEventArgs e)
		{
			if (!ReadOnly)
			{
				EventHandler<ActionEventArgs> te = ObjectPaste;
				if (te != null) te(this, e);
			}
		}
		public virtual void OnDataChanged(ActionEventArgs e)
		{
			EventHandler<ActionEventArgs> te = DataChanged;
			if (te != null) te(this, e);
		}
		public virtual void OnDataDeleted(ActionEventArgs e)
		{
			EventHandler<ActionEventArgs> te = DataDeleted;
			if (te != null) te(this, e);
		}
		#endregion Public Events

		#region Public Properties
		public string ObjectName { get; set; }
		public bool Enter2Tab { get; set; }
		public bool AllowAddObject { get; set; }
		public bool AllowEditObject { get; set; }
		public bool AllowDelObject { get; set; }
		public new bool ReadOnly
		{
			get
			{
				return !(AllowAddObject || AllowDelObject || AllowEditObject);
			}
			set
			{
				if (value)
					AllowEditObject = AllowAddObject = AllowDelObject = false;
			}
		}
		public DataTable BaseTable { get { return dt; } }
		//public void AddColumn(SortedList<int, AttribInfo> attInfo)
		//{
		//	foreach (KeyValuePair<int, AttribInfo> kvp in attInfo)
		//	{
		//		this.AddColumn(kvp.Value);
		//	}
		//}
		public void AddColumnsFromObjectSchema()
		{
			JMXSchema s = JMXSchemaProviderFactory.Default.GetSchema(this.ObjectName);
			foreach (var attrib in s.Attributes)
				AddColumn(attrib);
		}
		public void AddColumn(params JMXAttribute[] aiList)
		{
			foreach (var ai in aiList)
			{
				if (ai.Locate)
				{
					MacroType mt = DBGridBase.GetMacroType(ai.DataType);
					if (mt == MacroType.log)
					{
						DataGridViewCheckBoxColumn dck = new DataGridViewCheckBoxColumn();
						dck.Name = ai.AttribName;
						dck.HeaderText = ai.Name;
						dck.DataPropertyName = ai.FieldName;
						dck.SortMode = ai.Sorted ? DataGridViewColumnSortMode.Programmatic : DataGridViewColumnSortMode.NotSortable;
						dck.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
						dck.Visible = ai.Visible;
						this.Columns.Add(dck);
					}
					else if (ai.Mask.Equals("icon", StringComparison.CurrentCultureIgnoreCase))
					{
						DataGridViewImageColumn ic = new DataGridViewImageColumn(false);
						ic.DataPropertyName = ai.FieldName;
						ic.Visible = true;
						ic.Width = 50;
						this.Columns.Add(ic);
					}
					else if (ai.ListData.Count > 0)
					{
						DataGridViewComboBoxColumn cb = new DataGridViewComboBoxColumn();
						cb.Name = ai.AttribName;
						cb.HeaderText = ai.Name;
						cb.DataPropertyName = ai.FieldName;
						cb.SortMode = ai.Sorted ? DataGridViewColumnSortMode.Programmatic : DataGridViewColumnSortMode.NotSortable;
						cb.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
						DataTable dtList = GetItemDataList(ai.ListItems, ai.ListData, ai.DataType);
						cb.DataSource = dtList;
						cb.DisplayMember = dtList.Columns[0].ColumnName;
						cb.ValueMember = dtList.Columns[1].ColumnName;
						cb.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
						cb.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
						cb.Visible = ai.Visible;
						this.Columns.Add(cb);
					}
					else
					{
						DataGridViewColumn dc = this.Columns[this.Columns.Add(ai.AttribName, ai.Name)];
						dc.DataPropertyName = ai.FieldName;
						dc.SortMode = ai.Sorted ? DataGridViewColumnSortMode.Programmatic : DataGridViewColumnSortMode.NotSortable;
						dc.Visible = ai.Visible;
						//dc.Width = (int)(ai.DisplayWidth * this.ColumnHeadersDefaultCellStyle.Font.SizeInPoints) + 20;
						Font font = this.RowsDefaultCellStyle.Font ?? this.ColumnHeadersDefaultCellStyle.Font;
						dc.Width = (int)((ai.DisplayWidth) * font.SizeInPoints + font.Size * 3);

						if (AccntFormat.ValidFormat(ai.Format))
						{
							dc.DefaultCellStyle.Format = ai.Format;
							dc.DefaultCellStyle.FormatProvider = new AccntFormat();
						}
						else if (ai.Format.Contains("mm"))
							dc.DefaultCellStyle.Format = ai.Format.Replace(" mmmm ", " MMMM").Replace(" mmm ", " MMM").Replace(".mm.", ".MM.");
						else
							dc.DefaultCellStyle.Format = ai.Format;
						switch (mt)
						{
							case MacroType.num:
								dc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
								break;
							case MacroType.log:
								dc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
								break;
							case MacroType.date:
								dc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
								break;
							default:
								dc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
								break;
						}
					}
				}
			}
		}
		public Func<IObjectEditor> ObjectEditor { get; set; }
		#endregion Public Properties

		#region GridStyle
		public virtual GridSyle Style
		{
			get { return gs; }
			set
			{
				gs = value;
				if (gs == GridSyle.View)
				{
					this.AllowUserToAddRows = false;
					this.AllowUserToDeleteRows = false;
					this.EditMode = DataGridViewEditMode.EditProgrammatically;
					this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
					this.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
					this.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.InactiveCaptionText;
					RaiseCell = true;
				}
				else
				{
					this.AllowUserToAddRows = true;
					this.AllowUserToDeleteRows = true;
					this.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
					this.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
					this.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
					this.SelectionMode = DataGridViewSelectionMode.CellSelect;
				}
				//this.GridColor = Color.WhiteSmoke; // Color.FromArgb(204, 204, 204);
				this.BackgroundColor = this.GridColor;
				this.ForeColor = this.BackgroundColor;
				this.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
				this.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
				//this.RowHeadersDefaultCellStyle = this.RowTemplate.DefaultCellStyle.Clone();
				//this.RowHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
				//this.RowHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Control;
				//this.RowHeadersDefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.ControlText;
			}
		}
		#endregion GridStyle

		#region Raise Cells
		public virtual void CopyCellValue()
		{
			if (this.Rows.Count == 0) return;
			string fText = this.CurrentCell.Value.ToString().Replace(
				System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator, "");
			if (!string.IsNullOrEmpty(fText))
				Clipboard.SetText(fText);

		}

		public virtual bool RaiseCell
		{
			get { return raiseCell; }
			set
			{
				raiseCell = value;
				if (raiseCell)
				{
					this.CellPainting += new DataGridViewCellPaintingEventHandler(OnGridCellPainting);
				}
				else
				{
					this.CellPainting -= new DataGridViewCellPaintingEventHandler(OnGridCellPainting);
				}
			}
		}

		private void OnGridCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.ColumnIndex == -1 && e.RowIndex > -1 && _selectionStyle == DBGridSelectionStyle.CheckBox)
			{
				//Row header checkbox
				DataGridViewRow r = this.Rows[e.RowIndex];
				Point p = new Point();
				Size s = CheckBoxRenderer.GetGlyphSize(e.Graphics,
				System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
				p.X = e.CellBounds.Location.X +
					(e.CellBounds.Width / 2) - (s.Width / 2);
				p.Y = e.CellBounds.Location.Y +
					((e.CellBounds.Height - r.DividerHeight) / 2) - (s.Height / 2);
				System.Windows.Forms.VisualStyles.CheckBoxState _cbState = checkedRows.Contains(r) ?
					System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal :
					System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
				e.PaintBackground(e.ClipBounds, true);
				CheckBoxRenderer.DrawCheckBox(e.Graphics, p, _cbState);
				e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
				e.Handled = true;
			}
			else if (this.DataSource != null && this.Rows.Count > 0 && this.CurrentCellAddress.Y == e.RowIndex &&
				this.CurrentCellAddress.X == e.ColumnIndex)
			{
				Color color = this.RowTemplate.DefaultCellStyle.SelectionBackColor;
				e.CellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(color.R + 32, color.G + 32, color.B - 32);
				e.CellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			}
		}
		#endregion Raise Cells

		#region Footer
		public bool Footer
		{
			get { return footer; }
			set
			{
				footer = value;
				if (footer)
				{
					this.footerPanel.Visible = true;
					this.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.OnGridUserAddedRow);
					this.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.OnGridUserDeletedRow);
					this.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.OnGridDataBindingComplete);
					this.Resize += new System.EventHandler(this.OnDBGridResize);
					this.VisibleChanged += new System.EventHandler(this.OnGridVisibleChanged);
					this.ColumnWidthChanged += new DataGridViewColumnEventHandler(OnGridColumnsChanged);
					this.ColumnDisplayIndexChanged += new DataGridViewColumnEventHandler(OnGridColumnsChanged);
					this.ColumnAdded += new DataGridViewColumnEventHandler(OnGridColumnsAddedRemoved);
					this.ColumnRemoved += new DataGridViewColumnEventHandler(OnGridColumnsAddedRemoved);
					this.ColumnStateChanged += new DataGridViewColumnStateChangedEventHandler(OnGridColumnsStateChanged);
					this.RowHeadersWidthChanged += new EventHandler(OnRowHeadersWidthChanged);
					this.ColumnHeadersDefaultCellStyleChanged += new EventHandler(OnColumnHeadersDefaultCellStyleChanged);
					this.Scroll += new ScrollEventHandler(OnGridScroll);
					this.MouseMove += new MouseEventHandler(OnGridMouseMove);
					this.RowPostPaint += new DataGridViewRowPostPaintEventHandler(DBGridBase_RowPostPaint);
					RefreshTotals();
				}
				else
				{
					this.footerPanel.Visible = false;
					this.UserAddedRow -= new System.Windows.Forms.DataGridViewRowEventHandler(this.OnGridUserAddedRow);
					this.UserDeletedRow -= new System.Windows.Forms.DataGridViewRowEventHandler(this.OnGridUserDeletedRow);
					this.DataBindingComplete -= new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.OnGridDataBindingComplete);
					this.Resize -= new System.EventHandler(this.OnDBGridResize);
					this.VisibleChanged -= new System.EventHandler(this.OnGridVisibleChanged);
					this.ColumnWidthChanged -= new DataGridViewColumnEventHandler(OnGridColumnsChanged);
					this.ColumnDisplayIndexChanged -= new DataGridViewColumnEventHandler(OnGridColumnsChanged);
					this.ColumnAdded -= new DataGridViewColumnEventHandler(OnGridColumnsAddedRemoved);
					this.ColumnRemoved -= new DataGridViewColumnEventHandler(OnGridColumnsAddedRemoved);
					this.ColumnStateChanged -= new DataGridViewColumnStateChangedEventHandler(OnGridColumnsStateChanged);
					this.RowHeadersWidthChanged -= new EventHandler(OnRowHeadersWidthChanged);
					this.ColumnHeadersDefaultCellStyleChanged -= new EventHandler(OnColumnHeadersDefaultCellStyleChanged);
					this.Scroll -= new ScrollEventHandler(OnGridScroll);
					this.MouseMove -= new MouseEventHandler(OnGridMouseMove);
					this.RowPostPaint -= new DataGridViewRowPostPaintEventHandler(DBGridBase_RowPostPaint);
				}
			}
		}

		void DBGridBase_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			if ((e.State & DataGridViewElementStates.Selected) ==
						DataGridViewElementStates.Selected)
			{
				if (footer && e.RowIndex > 2 && e.RowBounds.Y + e.RowBounds.Height - 4 > footerPanel.Top && !lastRowPainted)
				{
					this.FirstDisplayedScrollingRowIndex++;
					//this.InvalidateRow(e.RowIndex - 2);
					this.InvalidateRow(e.RowIndex - 1);
					this.InvalidateRow(e.RowIndex);
				}
				lastRowPainted = !lastRowPainted;
			}
		}

		public string FooterCaption
		{
			get { return lblFooter.Text; }
			set
			{
				lblFooter.Text = value;
				if (lblFooter.Text.Length < 4)
					this.RowHeadersWidth = 4;
				else
					this.RowHeadersWidth = lblFooter.Text.Length * (int)lblFooter.Font.Size;
			}
		}
		public string FooterText(int index)
		{
			if (this.Columns.Count == 0 || !footer)
				return "";
			else
				return totalLabels[this.Columns[index]].Text;
		}
		public string FooterText(int index, string text)
		{
			if (this.Columns.Count == 0 || !footer) return "";

			Label label = totalLabels[this.Columns[index]];
			string old = label.Text;
			switch (this.Columns[index].DefaultCellStyle.Alignment)
			{
				case DataGridViewContentAlignment.NotSet:
					label.TextAlign = ContentAlignment.MiddleLeft;
					break;
				case DataGridViewContentAlignment.TopCenter:
					label.TextAlign = ContentAlignment.TopCenter;
					break;
				case DataGridViewContentAlignment.TopLeft:
					label.TextAlign = ContentAlignment.TopLeft;
					break;
				case DataGridViewContentAlignment.TopRight:
					label.TextAlign = ContentAlignment.TopRight;
					break;
				case DataGridViewContentAlignment.BottomCenter:
					label.TextAlign = ContentAlignment.BottomCenter;
					break;
				case DataGridViewContentAlignment.BottomLeft:
					label.TextAlign = ContentAlignment.BottomLeft;
					break;
				case DataGridViewContentAlignment.BottomRight:
					label.TextAlign = ContentAlignment.BottomRight;
					break;
				case DataGridViewContentAlignment.MiddleCenter:
					label.TextAlign = ContentAlignment.MiddleCenter;
					break;
				case DataGridViewContentAlignment.MiddleLeft:
					label.TextAlign = ContentAlignment.MiddleLeft;
					break;
				case DataGridViewContentAlignment.MiddleRight:
					label.TextAlign = ContentAlignment.MiddleRight;
					break;

			}
			totalLabels[this.Columns[index]].Text = text;
			return old;
		}
		public ContextMenuStrip FooterContextMenu
		{
			get { return mnuTotal; }
			set
			{
				mnuTotal = value;
				foreach (DataGridViewColumn c in this.Columns)
				{
					Label label = totalLabels[c];
					if (label != null)
					{
						label.ContextMenuStrip = mnuTotal;
					}
				}
			}
		}
		#region FooterSupport
		private void RefreshTotals()
		{
			lastRowHieght();
			addTotalLabels();
			sizeTotalLabel();
		}
		private void lastRowHieght()
		{
			if (footer)
			{
				int count = this.Rows.Count;
				if (count > 0 && this.Rows[count - 1].DividerHeight != this.RowTemplate.Height)
				{
					if (count > 1 && this.Rows[count - 2].DividerHeight != this.RowTemplate.DividerHeight)
					{
						this.Rows[count - 2].Height = this.RowTemplate.Height;
						this.Rows[count - 2].DividerHeight = this.RowTemplate.DividerHeight;
					}
					else if (this.Rows[count - 1].Height == this.RowTemplate.Height)
					{
						this.Rows[count - 1].Height *= 2;
						this.Rows[count - 1].DividerHeight = this.RowTemplate.Height;
					}
				}
			}
		}
		private void addTotalLabels()
		{
			totalLabels = new Dictionary<DataGridViewColumn, Label>();
			for (; this.footerPanel.Controls.Count > 0;)
				this.footerPanel.Controls.Remove(this.footerPanel.Controls[0]);

			this.footerPanel.Controls.Add(lblFooter);
			foreach (DataGridViewColumn c in this.Columns)
			{
				Label label = new Label();
				label.Top = 1;
				label.Height = this.footerPanel.Height - 1;
				label.BorderStyle = (BorderStyle)this.ColumnHeadersBorderStyle;
				label.ContextMenuStrip = mnuTotal;
				label.Tag = c;
				label.Font = this.RowsDefaultCellStyle.Font ?? this.ColumnHeadersDefaultCellStyle.Font; ;
				this.footerPanel.Controls.Add(label);
				totalLabels.Add(c, label);
			}
			sizeTotalLabel();
		}
		private void sizeTotalLabel()
		{
			if (!this.Visible || this.Columns.Count == 0 || !footer) return;
			try
			{
				this.SuspendLayout();
				int margin = this.Margin.Left;
				this.footerPanel.Location = new Point(margin, this.ClientSize.Height - this.footerPanel.Height -
					(this.HorizontalScrollBar.Visible ? this.HorizontalScrollBar.Height : 0));
				this.footerPanel.Width = this.Width;
				this.footerPanel.Visible = footer;

				int rhw = (this.RowHeadersVisible ? this.RowHeadersWidth : 0) - margin;
				int pos = rhw;

				if (rhw > 0)
				{
					lblFooter.Width = rhw;
					lblFooter.Visible = true;
				}
				else if (lblFooter.Visible)
					lblFooter.Visible = false;

				foreach (DataGridViewColumn c in this.Columns)
				{
					Label label = totalLabels[c];
					if (label != null)
					{
						if (!c.Visible)
						{
							if (label.Visible) label.Visible = false;
							continue;
						}
						int from = pos - this.HorizontalScrollingOffset;
						int width = c.Width;
						if (from < rhw)
						{
							width -= rhw - from;
							from = rhw;
						}
						if (from + width > this.footerPanel.Width)
							width = this.footerPanel.Width - from;
						if (width < 4)
						{
							if (label.Visible) label.Visible = false;
						}
						else
						{
							label.SetBounds(from, 0, width, 0, BoundsSpecified.X | BoundsSpecified.Width);
							label.Visible = true;
						}
						pos += c.Width;
					}
				}

			}
			finally
			{
				this.ResumeLayout();
			}
			this.footerPanel.Refresh();
			this.Invalidate();
		}
		void OnGridMouseMove(object sender, MouseEventArgs e)
		{

			if (e.Button == MouseButtons.Left && e.Y >= 1 &&
				e.Y <= this.ColumnHeadersHeight)
			{
				this.footerPanel.Refresh();
			}
		}
		private void OnDBGridResize(object sender, EventArgs e)
		{
			sizeTotalLabel();
		}
		private void OnGridVisibleChanged(object sender, EventArgs e)
		{
			sizeTotalLabel();
		}
		private void OnGridScroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
			{
				sizeTotalLabel();
			}
		}
		private void OnGridDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			checkedRows.Clear();
			lastRowHieght();
		}
		private void OnGridUserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			lastRowHieght();
		}
		private void OnGridUserDeletedRow(object sender, DataGridViewRowEventArgs e)
		{
			lastRowHieght();
		}
		private void OnRowHeadersWidthChanged(object sender, EventArgs e)
		{
			sizeTotalLabel();
		}
		private void OnColumnHeadersDefaultCellStyleChanged(object sender, EventArgs e)
		{
			if (this.ColumnHeadersDefaultCellStyle.BackColor != this.footerPanel.BackColor)
				this.footerPanel.BackColor = this.ColumnHeadersDefaultCellStyle.BackColor;
		}
		private void OnGridColumnsAddedRemoved(object sender, DataGridViewColumnEventArgs e)
		{
			addTotalLabels();
		}
		private void OnGridColumnsChanged(object sender, DataGridViewColumnEventArgs e)
		{
			sizeTotalLabel();
		}
		private void OnGridColumnsStateChanged(object sender, DataGridViewColumnStateChangedEventArgs e)
		{
			if (e.StateChanged == DataGridViewElementStates.Visible)
				sizeTotalLabel();
		}
		#endregion FooterSupport
		#endregion Footer

		#region Search
		public virtual void GoTo(int rowNumber)
		{
			if (this.Rows.Count == 0) return;
			int offset = (this.CurrentCellAddress.Y - this.FirstDisplayedScrollingRowIndex);
			this.CurrentCell = this.Rows[rowNumber].Cells[this.CurrentCellAddress.X];
			if (offset > 0 && rowNumber - offset >= 0)
				this.FirstDisplayedScrollingRowIndex = rowNumber - offset;
			else if (rowNumber < this.Rows.Count)
				this.FirstDisplayedScrollingRowIndex = rowNumber;
		}
		public virtual void Find(Func<DataRow, bool> fc)
		{
			Find(fc, 1);
		}
		public virtual void Find(Func<DataRow, bool> fc, int startIndex)
		{
			//startIndex += 1;
			if (string.IsNullOrEmpty(dt.DefaultView.RowFilter))
			{
				DataRowCollection rows = dt.Rows;
				//System.Threading.Tasks.Parallel.For(startIndex, rows.Count, (i, loopState) =>
				for (int i = startIndex; i < rows.Count; i++)
				{
					if (fc(rows[i]))
					{
						this.GoTo(i);
						//loopState.Break();
						break;
					}
				}//);
			}
			else
			{
				DataView dv = dt.DefaultView;
				for (int i = startIndex; i < dv.Count; i++)
				{
					if (fc(dv[i].Row))
					{
						this.GoTo(i);
						break;
					}
				}
			}
		}
		public virtual void RefreshAll()
		{
			dt.DefaultView.RowFilter = "";
			base.Refresh();
		}
		public virtual void RefreshAll(object bookmark)
		{
			this.RefreshAll();
			if (bookmark.GetType() == typeof(JMXObject))
			{
				int id = (bookmark as JMXObject).ID;
				Find(r => (int)r["ID"] == id);
			}
			else
				this.GoTo(Convert.ToInt32(bookmark));
		}
		public virtual void Reload()
		{
			this.RefreshAll();
		}
		protected virtual void DoFindFirst()
		{
			//if (this.Rows.Count == 0) return;
			//DataGridViewColumn col = this.Columns[this.CurrentCellAddress.X];

			//using (CDialog cd = new CDialog(CDialogPageStyle.SinglePage, CDialogStyle.Dialog))
			//{
			//	TableLayoutPanel tp = cd.AddCells("WorkTable", 0, new System.Drawing.Size(4, 1), CDialogCellStyle.Proportional);

			//	cd.AddStdButtons("OK", "Cancel");
			//	cd.GetControl<Button>("OK").Text = "Найти";
			//	if (vbo.IsNumericType(col.ValueType))
			//	{
			//		cd.Add(new CDialogItem("Condition", col.HeaderText)
			//		{
			//			ListItems = new List<string>() { "=", ">", "<" },
			//			Mask = "Lock",
			//			Value = string.IsNullOrEmpty(lastCondition) || !"=><".Contains(lastCondition) ? "=" : lastCondition
			//		});
			//		cd.Add(new CDialogItem("Value", string.Empty)
			//		{
			//			PresentationType = typeof(ComboBox),
			//			DataType = col.ValueType,
			//			Value = vbo.IsNull(this.CurrentCell.Value, 0)
			//		});
			//		ComboBox numTB = cd.GetControl<ComboBox>("Value");
			//		numTB.Width = 120;
			//		setAutoComplete(numTB);
			//		cd.Activated += new EventHandler((o, e) => { ((CDialog)o).GetControl<ComboBox>("Value").Focus(); });
			//		tp.SetColumnSpan(numTB, 2);
			//	}
			//	else if (col.ValueType == typeof(DateTime))
			//	{
			//		cd.Add(new CDialogItem("Condition", col.HeaderText)
			//		{
			//			ListItems = new List<string>() { "=", ">", "<" },
			//			Mask = "Lock",
			//			Value = string.IsNullOrEmpty(lastCondition) || !"=><".Contains(lastCondition) ? "=" : lastCondition
			//		});
			//		cd.Add(new CDialogItem("Value", string.Empty) { DataType = typeof(DateTime), Value = vbo.IsNull(this.CurrentCell.Value, vbo.Date()) });
			//		DateEdit de = cd.GetControl<DateEdit>("Value");
			//		de.Width = 100;
			//		cd.Activated += new EventHandler((o, e) => { ((CDialog)o).GetControl<DateEdit>("Value").Focus(); });
			//		tp.SetColumnSpan(de, 2);
			//	}
			//	else
			//	{
			//		if (col.ValueType == typeof(bool))
			//		{
			//			cd.Add(new CDialogItem("Condition", col.HeaderText)
			//			{
			//				ListItems = new List<string>() { "=" },
			//				Mask = "Lock",
			//				Value = "="
			//			});
			//		}
			//		else
			//		{
			//			cd.Add(new CDialogItem("Condition", col.HeaderText)
			//			{
			//				ListItems = new List<string>() { "Like", "=" },
			//				Mask = "Lock",
			//				Value = string.IsNullOrEmpty(lastCondition) || !"Like=".Contains(lastCondition) ? "Like" : lastCondition
			//			});
			//		}
			//		cd.Add(new CDialogItem("Value", string.Empty)
			//		{
			//			PresentationType = typeof(ComboBox),
			//			DataType = col.ValueType,
			//			Value = this.CurrentCell.Value
			//		});
			//		ComboBox cbox = cd.GetControl<ComboBox>("Value");
			//		cbox.Width = 120;
			//		setAutoComplete(cbox);
			//		cd.Activated += new EventHandler((o, e) => { ((CDialog)o).GetControl<ComboBox>("Value").Focus(); });
			//		tp.SetColumnSpan(cbox, 2);
			//	}
			//	cd.Add(new CDialogItem("FindAll", "") { Value = false, DataType = typeof(bool), PresentationType = typeof(CheckBox) }, cd.GetCells(0), new CDialogAddress(2, 1));
			//	cd.GetControl<CheckBox>("FindAll").Text = "Найти все";
			//	cd.AcceptButton = cd.GetControl<Button>("OK");
			//	cd.CancelButton = cd.GetControl<Button>("Cancel");
			//	cd.Size = new Size(550, (int)(550 / (vbo.GoldenRatio * 2)));
			//	if (cd.ShowDialog() == DialogResult.OK)
			//	{
			//		cd.Save();
			//		lastSearch = cd.GetItem("Value").Value;
			//		string ls = lastSearch.ToString();
			//		lastCondition = cd.GetItem("Condition").Value.ToString();
			//		if (searches.Contains(ls)) searches.Remove(ls);
			//		searches.Insert(0, ls);
			//		if (!(bool)cd.GetItem("FindAll").Value)
			//			DoConditionFind(1);
			//		else
			//			conditionFindAll();
			//	}
			//	cd.Dispose();
			//}
		}
		protected virtual void DoConditionFind(int rowIndex)
		{
			//if (this.Rows.Count == 0) return;
			//string colName = this.Columns[this.CurrentCellAddress.X].Name;
			//if (lastCondition == "Like")
			//{
			//	string template = lastSearch.ToString().ToLower();
			//	this.Find(dr => vbo.Like(dr[colName].ToString().ToLower(), template), rowIndex);
			//}
			//else if (lastCondition == "=")
			//{
			//	this.Find(dr => dr[colName].Equals(lastSearch), rowIndex);
			//}
			//else if (lastSearch.GetType() == typeof(DateTime))
			//{
			//	if (lastCondition == "<")
			//		this.Find(dr => (DateTime)vbo.IsNull(dr[colName], DateTime.MinValue) < (DateTime)lastSearch, rowIndex);
			//	else
			//		this.Find(dr => (DateTime)vbo.IsNull(dr[colName], DateTime.MinValue) > (DateTime)lastSearch, rowIndex);
			//}
			//else if (vbo.IsNumericType(lastSearch.GetType()))
			//{
			//	lastSearch = Convert.ToDecimal(lastSearch);
			//	if (lastCondition == "<")
			//		this.Find(dr => Convert.ToDecimal(vbo.IsNull(dr[colName], decimal.MinValue)) < (decimal)lastSearch, rowIndex);
			//	else
			//		this.Find(dr => Convert.ToDecimal(vbo.IsNull(dr[colName], decimal.MinValue)) > (decimal)lastSearch, rowIndex);
			//}
		}
		private void conditionFindAll()
		{
			if (this.Rows.Count == 0) return;
			string colName = this.Columns[this.CurrentCellAddress.X].Name;
			DataView dv = dt.DefaultView;
			if (lastCondition == "Like")
			{
				string template = lastSearch.ToString().ToLower();
				dv.RowFilter = colName + " Like '%" + template + "%'";
			}
			else if (lastSearch.GetType() == typeof(string))
				dv.RowFilter = colName + " " + lastCondition + " '" + lastSearch.ToString() + "'";
			else if (lastSearch.GetType() == typeof(DateTime))
				dv.RowFilter = colName + " " + lastCondition + " #" + ((DateTime)lastSearch).ToString(vbo.SortDateFormat) + "# ";
			else
				dv.RowFilter = colName + " " + lastCondition + " " + lastSearch.ToString().Replace(',', '.');
		}
		private static void setAutoComplete(ComboBox tb)
		{
			tb.Items.AddRange(searches.ToArray());
			tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			tb.AutoCompleteSource = AutoCompleteSource.ListItems;
		}
		public void SetSpeedSearch(ToolStripTextBox textBox)
		{
			_sss = new GridSpeedSearch(this, textBox);
		}
		public GridSpeedSearch GetSpeedSearch() => _sss;

		#endregion Search

		#region Row Selection support
		public DBGridSelectionStyle SelectionStyle
		{
			get { return _selectionStyle; }
			set
			{
				_selectionStyle = value;
				this.MultiSelect = (value != DBGridSelectionStyle.CheckBox);
			}
		}

		public DataRow[] Selection()
		{
			List<DataRow> listRows = new List<DataRow>();
			foreach (DataGridViewRow gridRow in this.SelectedRows)
			{
				listRows.Add(this.getRow(gridRow.Index));
			}
			if (listRows.Count == 0)
				return null;
			return listRows.ToArray();
		}

		public DataRow DataRow
		{
			get
			{
				if (this.Rows.Count == 0)
					return null;
				else
					return getRow(this.CurrentCellAddress.Y);
			}
		}

		protected DataRow getRow(int index)
		{
			if (string.IsNullOrEmpty(dt.DefaultView.RowFilter))
				return dt.Rows[index];
			else
				return dt.DefaultView[index].Row;
		}

		public new void SelectAll()
		{
			if (_selectionStyle == DBGridSelectionStyle.Normal)
			{
				if (this.Rows.Count > 0)
				{
					if (string.IsNullOrEmpty(dt.DefaultView.RowFilter))
						this.GoTo(this.Rows.Count - 1);
					else
						this.GoTo(dt.DefaultView.Count - 1);
				}
				base.SelectAll();
			}
			else
			{
				checkedRows.Clear();
				foreach (DataGridViewRow r in this.Rows)
				{
					checkRow(r);
				}
				this.Refresh();
			}
		}

		public new IList<DataGridViewRow> SelectedRows
		{
			get
			{
				if (_selectionStyle == DBGridSelectionStyle.CheckBox && (this.Rows.Count == 0 || checkedRows.Count > 0))
					return checkedRows;

				List<DataGridViewRow> rr = new List<DataGridViewRow>(base.SelectedRows.Count);
				foreach (DataGridViewRow r in base.SelectedRows)
				{
					rr.Add(r);
				}
				return rr;
			}
		}
		#endregion Row Selection support

		#region Read&Edit Objects
		public JMXObject ReadObject()
		{
			return ReadObject(false);
		}
		public JMXObject ReadObject(bool createNew)
		{
			if (this.Rows.Count == 0) createNew = true;
			ActionEventArgs e = new ActionEventArgs();
			if (createNew)
				OnObjectReadNew(e);
			else
				OnObjectRead(e);
			return e.ObjSource;
		}
		public virtual JMXObject DoObjectRead(bool isNew)
		{
			JMXObject jObj = new JMXObject(this.ObjectName);

			if (!isNew)
			{
				var dr = getRow(this.CurrentCellAddress.Y);
				foreach (DataGridViewColumn dc in this.Columns)
					if (!dc.Name.IsEmpty())
						jObj[dc.Name] = JToken.FromObject(dr[dc.Name]);
			}
			return jObj;
		}
		protected virtual void DoObjectEditAdd(JMXObject objSource, bool isNew)
		{
			EditorFactory.ObjectEdit(objSource, isNew, this, null, editorRefreshHost, this.ObjectEditor == null ? null : this.ObjectEditor());
		}
		protected virtual void DoObjectEditAdd(JMXObject objSource, bool isNew, string actionID)
		{
			EditorFactory.ObjectEdit(objSource, isNew, this, actionID, editorRefreshHost, this.ObjectEditor == null ? null : this.ObjectEditor());
		}
		protected virtual void DoObjectDel(JMXObject objSource)
		{
			if (this.Rows.Count != 0)
			{
				var r = FindRow(objSource.ID);
				if (r != null)
				{
					this.BaseTable.Rows.Remove(r);
					OnDataDeleted(new ActionEventArgs(objSource));
				}
			}
		}
		public DataRow FindRow(int ibjectID)
		{
			foreach (DataRow dr in dt.Rows)
				if ((int)dr["ID"] == ibjectID)
					return dr;
			return null;
		}
		#endregion Read&Edit Objects

		#region Moving
		public void MoveDown()
		{
			this.MoveRow(1);
		}
		public void MoveUp()
		{
			this.MoveRow(-1);
		}

		public void MoveRow(int skipRows)
		{
			if (!this.AllowEditObject || this.Rows.Count == 0 || skipRows == 0)
				return;
			int currentIndex = this.CurrentCellAddress.Y;
			int newRowIndex = skipRows > 0 ? Math.Min(this.Rows.Count - 1, currentIndex + skipRows) :
				Math.Max(0, currentIndex + skipRows);
			if ((newRowIndex - currentIndex) == 0)
				return;

			DataRow currentRow = this.DataRow;
			DataRow buffer = currentRow.Clone();
			DataRow nextRow = this.getRow(newRowIndex);
			nextRow.CopyTo(currentRow);
			buffer.CopyTo(nextRow);
			this.CurrentCell = this.Rows[newRowIndex].Cells[this.CurrentCellAddress.X];
			OnDataChanged(new ActionEventArgs());
		}
		#endregion Moving

		#region Event Implements
		private void DBGridBase_CurrentCellChanged(object sender, EventArgs e)
		{
			if (this.CurrentCell != null)
			{
				if (oldCol != this.CurrentCellAddress.X)
				{
					lastSearch = null;
					lastCondition = string.Empty;
					oldCol = this.CurrentCellAddress.X;
				}
			}

		}

		private void DBGridBase_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.F && e.Control) || (e.KeyCode == Keys.F3 && lastSearch == null))
				OnFindFirst(new ActionEventArgs(null));
			else if (e.KeyCode == Keys.Down && e.Control)
			{
				this.MoveRow(1);
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Up && e.Control)
			{
				this.MoveRow(-1);
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.OemMinus)
			{
				this.RefreshAll();
			}
			else if (e.Control || e.Shift || e.Alt) { }
			else if (e.KeyCode == Keys.F3)
				OnFindNext(new ActionEventArgs(null));
			else if (e.KeyCode == Keys.Enter && this.Enter2Tab)
			{
				SendKeys.Send("{TAB}");
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.F4)
			{
				if (this.Rows.Count == 0)
					OnObjectAdd(new ActionEventArgs(this.ReadObject(true)));
				else
					OnObjectEdit(new ActionEventArgs(this.ReadObject(false)));
			}
			else if (e.KeyCode == Keys.Insert)
			{
				OnObjectAdd(new ActionEventArgs(this.ReadObject(true)));
			}
			else if (e.KeyCode == Keys.Delete)
			{
				if (this.Rows.Count > 0)
					OnObjectDel(new ActionEventArgs(this.ReadObject(false)));
			}
			else if (e.KeyCode == Keys.Space)
			{
				if (this.Rows.Count > 0)
					CheckRow(this.Rows[this.CurrentCellAddress.Y]);
			}
		}

		void DBGridBase_DataSourceChanged(object sender, EventArgs e)
		{
			if (this.DataSource is DataTable)
				dt = (DataTable)this.DataSource;
			else if (this.DataSource is DataSet)
				dt = (this.DataSource as DataSet).Tables[this.DataMember];
			else
				dt = (DataTable)this.DataSource;
		}

		void editorRefreshHost(IObjectEditor oe)
		{
			var jObj = oe.EditObject;
			OnDataChanged(new ActionEventArgs(jObj));
			this.RefreshAll(jObj);
		}
		#endregion Event Implements

		#region Public Static Methods
		public static MacroType GetMacroType(MdbType type)
		{
			Type t = MdbTypeMap.GetType(type);
			if (t.IsNumeric())
				return MacroType.num;
			else if (t == typeof(bool))
				return MacroType.log;
			else if (t == typeof(DateTime))
				return MacroType.date;
			else
				return MacroType.str;
		}
		public static DataTable GetItemDataList(List<string> items, List<object> data, MdbType dataType)
		{
			DataTable dtList = new DataTable("ListItems");
			Type type = MdbTypeMap.GetType(dataType);
			dtList.Columns.Add("ListItem", typeof(string));
			dtList.Columns.Add("ListData", type);
			for (int i = 0; i < items.Count; i++)
			{
				DataRow dr = dtList.NewRow();
				dr[0] = items[i];
				dr[1] = data[i];
				dtList.Rows.Add(dr);
			}
			return dtList;
		}
		#endregion Public Static Methods

		protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false; ;
		}
	}
}
