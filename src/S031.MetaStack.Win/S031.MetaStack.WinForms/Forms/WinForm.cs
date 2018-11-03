using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Windows.Forms;
using System.Drawing;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public enum WinFormCellStyle
	{
		AutoSize = 0,
		Proportional = 1,
		CustomSize = 2
	}

	public enum WinFormStyle
	{
		Form = 0,
		Dialog = 1
	}

	public sealed class WinFormRef
	{
		public WinForm WinForm { get; internal set; }
		public WinFormItem WinFormItem { get; internal set; }
	}

	public class DataChangedEventArgs : EventArgs
	{
		public Control SourceControl { get; set; }
		public DataChangedEventArgs() { }
		public DataChangedEventArgs(Control sourceCtrl) { SourceControl = sourceCtrl; }
	}
	public class ItemAddEventArgs : EventArgs
	{
		public WinFormItem Item { get; set; }
		public ItemAddEventArgs() { }
		public ItemAddEventArgs(WinFormItem item) { Item = item; }
	}

	public class WinForm : Form
	{
		public event EventHandler<ItemAddEventArgs> ItemAdd;
		public event EventHandler<ItemAddEventArgs> ItemAdded;
		public void OnItemAdd(WinFormItem item)
		{
			ItemAdd?.Invoke(this, new ItemAddEventArgs(item));
		}
		public void OnItemAdded(WinFormItem item)
		{
			ItemAdded?.Invoke(this, new ItemAddEventArgs(item));
		}

		#region Module Variables
		//Constants
		public const string FileSaveForm = "filesave";
		public const string FileOpenForm = "fileopen";
		public const string FolderBrowseForm = "folderbrowse";
		public const string StrViewForm = "strview";
		public const string StrViewFormList = "strviewlist";
		public const int MinFormWidth = 500;

		//Size defaults constants
		public const TableLayoutPanelCellBorderStyle CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
		public const int CellsRowHieght = 20;
		public const int ButtonHeight = 35;
		public const int ButtonWidth = 100;

		//New object
		bool _isNew;

		//WinForm Styles
		private WinFormStyle _dialogStyle;

		//Main Panels
		private TabControlEx _tc = default;
		//Form rows panels (any panels for form)
		private readonly List<TableLayoutPanel> _formRowsPanels = new List<TableLayoutPanel>();
		//Pages rows panels one panel (any rows) for page
		private readonly List<TableLayoutPanel> _pagesRowsPanels = new List<TableLayoutPanel>();
		//private TableLayoutPanel _buttonTablePanel;

		//Error shown tooltip
		private ToolTip _errToolTip;

		//WinFormItems container
		private Dictionary<string, WinFormItem> _items = new Dictionary<string, WinFormItem>(StringComparer.CurrentCultureIgnoreCase);
		public Dictionary<string, WinFormItem> Items => _items;

		// CodeCompletion Support
		private readonly Dictionary<string, string> _codeCompletionInfo = new Dictionary<string, string>();
		private CodeCompletionWindow _codeCompletionWindow = null;
		private bool _controlPressed;
		private char _quoteChar;

		//Multi selection
		private string _separator = vbo.vbSep;
		# endregion Module Variables

		#region Constructor
		public WinForm() : this(WinFormStyle.Dialog) { }
		public WinForm(WinFormStyle ds)
		{
			//FontManager.Adop(this);
			this.Id = Guid.NewGuid();
			this.KeyPreview = true;
			this.AutoSize = true;
			this.MinimumSize = new Size(ButtonWidth * 4, ButtonHeight + this.FontHeight * 2);
			//this.KeyDown += new KeyEventHandler(WinForm_KeyDown);
			this.RefreshHostOnExit = true;
			this.DialogStyle = ds;
			//ErrToolTip
			_errToolTip = new ToolTip
			{
				ToolTipIcon = ToolTipIcon.Error
			};
		}
		#endregion Constructor

		#region PublicProperties
		public string Separator { get { return _separator; } set { _separator = value; } }

		public WinFormStyle DialogStyle
		{
			get { return _dialogStyle; }
			set
			{
				_dialogStyle = value;
				if (value == WinFormStyle.Dialog)
				{
					this.ShowInTaskbar = false;
					this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
				}
				else
				{
					this.ShowInTaskbar = true;
					this.FormBorderStyle = FormBorderStyle.Sizable;
				}
			}
		}

		public TabControl TabControl => _tc;

		public bool IsNew
		{
			get { return _isNew; }
			set
			{
				_isNew = value;
				if (value)
					this.ContinuousEditing = true;
			}
		}

		public bool SaveInDB { get; set; }

		public Guid Id { get; private set; }

		public bool ContinuousEditing { get; set; }

		public bool RefreshHostOnExit { get; set; }

		public bool RefreshHostOnSaved { get; set; }

		public string NewName(Type type)
		{
			string n = type.Name;
			string orig = n;
			for (int i = 1; _items.ContainsKey(n); i++)
				n = orig + i.ToString();
			return n;
		}

		#endregion PublicProperties

		#region Events
		public event EventHandler<DataChangedEventArgs> DataChanged;

		public virtual void OnDataChanged(DataChangedEventArgs e)
		{
			DataChanged?.Invoke(this, e);
		}
		#endregion Events

		#region Protected
		public virtual void OnItemButtonClick(WinFormItem cdi)
		{
			if (cdi == null || string.IsNullOrEmpty(cdi.SuperForm))
				return;
			else if (cdi.SuperForm.ToLower() == WinForm.FileSaveForm)
			{
				using (SaveFileDialog dlg = new SaveFileDialog())
				{
					dlg.FileName = string.IsNullOrEmpty(cdi.SuperMethod) ? cdi.Caption : cdi.SuperMethod;
					if (!cdi.SuperFilter.IsEmpty())
						dlg.Filter = cdi.SuperFilter;
					dlg.OverwritePrompt = false;
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						cdi.LinkedControl.Text = dlg.FileName;
						cdi.LinkedControl.Focus();
					}
					else
						SendKeys.Send("{TAB}");
				}
			}
			else if (cdi.SuperForm.ToLower() == WinForm.FileOpenForm)
			{
				using (OpenFileDialog dlg = new OpenFileDialog())
				{
					dlg.FileName = string.IsNullOrEmpty(cdi.LinkedControl.Text) ? cdi.SuperMethod : cdi.LinkedControl.Text;
					if (!cdi.SuperFilter.IsEmpty())
						dlg.Filter = cdi.SuperFilter;
					dlg.CheckFileExists = false;
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						cdi.LinkedControl.Text = dlg.FileName;
						cdi.LinkedControl.Focus();
					}
					else
						SendKeys.Send("{TAB}");
				}
			}
			else if (cdi.SuperForm.ToLower() == WinForm.FolderBrowseForm)
			{
				using (FolderBrowserDialog dlg = new FolderBrowserDialog
				{
					SelectedPath = string.IsNullOrEmpty(cdi.LinkedControl.Text) ? cdi.SuperMethod : cdi.LinkedControl.Text
				})
				{
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						cdi.LinkedControl.Text = dlg.SelectedPath;
						cdi.LinkedControl.Focus();
					}
					else
						SendKeys.Send("{TAB}");
				}
			}
			else if (cdi.SuperForm.Left(7).Equals(WinForm.StrViewForm, StringComparison.CurrentCultureIgnoreCase))
			{
				string buff;
				if (cdi.SuperForm.Length == WinForm.StrViewForm.Length)
				{
					if ((buff = ShowStringDialog(cdi.LinkedControl.Text, string.Empty)) != null)
						cdi.LinkedControl.Text = buff;
				}
				else if (cdi.SuperForm.Equals(WinForm.StrViewFormList, StringComparison.CurrentCultureIgnoreCase))
				{
					if ((buff = WinForm.ShowStringDialog(cdi.LinkedControl.Text.Replace(",", vbo.vbCrLf), string.Empty)) != null)
					{
						buff = buff.Replace(vbo.vbCrLf, ",");
						for (; buff.Contains(",,");)
						{
							buff = buff.Replace(",,", ",");
						}
						if (buff.EndsWith(","))
							buff = buff.Substring(0, buff.Length - 1);
						cdi.LinkedControl.Text = buff;
						cdi.LinkedControl.Focus();
						//SendKeys.Send("{LEFT}");
					}
				}
				else
				{
					string language = cdi.SuperForm.Substring(WinForm.StrViewForm.Length).ToUpper();
					if (language == "BASIC")
						language = "VBNET";
					if ((buff = ShowStringDialog(cdi.LinkedControl.Text, language)) != null)
						cdi.LinkedControl.Text = buff;
				}
			}
			else
			{
				ShowBrowse(cdi, string.Empty);
			}
		}

		protected virtual object[] ShowBrowse(WinFormItem cdi, string filter, params string[] fieldListToReturn)
		{
			object[] result = null;
			//XMLForm xf = new XMLForm();

			//if (xf.Read(cdi.SuperForm))
			//{
			//	string key = cdi.LinkedControl.Text;
			//	bool qualifier = (cdi.SuperForm.ToLower() == "qualifier" && vbo.IsNumeric(cdi.SuperMethod));
			//	if (qualifier)
			//		xf.SetFilter("Qualifiers.QualifierID = " + cdi.SuperMethod);
			//	else if (!string.IsNullOrEmpty(filter))
			//		xf.SetFilter(filter);
			//	else if (string.IsNullOrEmpty(key) || key == "0") { }
			//	else if (cdi.DataType != typeof(string))
			//		xf.SetFilter(vbo.IsNumeric(key) ? cdi.SuperMethod + " = " + key : "Name" + " LIKE '%" + key + "%'");
			//	else
			//		xf.SetFilter(cdi.SuperMethod + " LIKE '" + key + "%'");
			//	if (!string.IsNullOrEmpty(cdi.SuperFilter))
			//	{
			//		if (cdi.SuperFilter.Left(1) == "=")
			//		{
			//			string filterExp = (string)Evaluator.Eval(cdi.SuperFilter.Substring(1));
			//			if (!string.IsNullOrEmpty(filter))
			//				xf.SetFilter(filterExp);
			//		}
			//		else
			//			xf.SetFilter(cdi.SuperFilter);
			//	}
			//	DBBrowseDialog dbDial = new DBBrowseDialog(xf);
			//	if (dbDial.LoadComplete)
			//	{
			//		if (qualifier)
			//		{
			//			byte search;
			//			if (byte.TryParse(key, out search))
			//				dbDial.FindRecord((dr) => dr["ClassID"].Equals(search));
			//		}
			//		if (dbDial.ShowDialog() == DialogResult.OK && dbDial.SelectedRow != null)
			//		{
			//			DataRow dr = dbDial.SelectedRow;
			//			if (dr.Table.Columns.Contains(cdi.SuperMethod))
			//			{
			//				if (dbDial.Selection().Length > 0)
			//				{
			//					System.Text.StringBuilder sb = new System.Text.StringBuilder();
			//					foreach (DataRow dr1 in dbDial.Selection())
			//					{
			//						sb.Append((dr1[cdi.SuperMethod] ?? string.Empty).ToString() + _separator);
			//					}
			//					cdi.LinkedControl.Text = sb.ToString(0, sb.Length - _separator.Length);
			//				}
			//				else
			//					cdi.LinkedControl.Text = (dr[cdi.SuperMethod] ?? string.Empty).ToString(); ;
			//				SendKeys.Send("{LEFT}");
			//			}
			//			else
			//			{
			//				XMLClient obj = new XMLClient(xf["ObjectName"]);
			//				if (obj.ReadByHandle(xf["ObjectName"], (int)vbo.Convert(typeof(int), dr["Handle"])))
			//				{
			//					if (qualifier)
			//					{
			//						cdi.LinkedControl.Text = obj["ClassID"];
			//						SendKeys.Send("{LEFT}");
			//					}
			//					else
			//					{
			//						try
			//						{
			//							cdi.LinkedControl.Text = obj[cdi.SuperMethod];
			//							SendKeys.Send("{LEFT}");
			//						}
			//						catch (ElementPathNotFoundException)
			//						{
			//							ShowToolTip("Невозможно выполнение метода: " + cdi.SuperMethod + " объекта: " + xf["ObjectName"], cdi.LinkedControl);
			//						}
			//					}
			//				}
			//				else
			//					ShowToolTip("Невозможно создание экземпляра объекта: " + xf["ObjectName"], cdi.LinkedControl);
			//			}
			//			List<object> resultSet = new List<object>();
			//			foreach (string field in fieldListToReturn)
			//			{
			//				if (dr.Table.Columns.Contains(field))
			//					resultSet.Add(dr[field]);
			//				else
			//					resultSet.Add(null);
			//			}
			//			result = resultSet.ToArray();
			//		}
			//		dbDial.Dispose();
			//	}
			//}
			return result;
		}
		#endregion Protected

		#region Public
		public T GetControl<T>(string name) where T : Control
		{
			if (_items.ContainsKey(name))
				return (_items[name].LinkedControl as T);
			else
				return null;
		}
		public WinFormItem GetItem(string name)
		{
			return _items[name];
		}
		public TableLayoutPanel GetCells()
		{
			return this.GetCells(0);
		}
		public TableLayoutPanel GetCells(int pageNumber)
		{
			return (_pagesRowsPanels[pageNumber].GetControlFromPosition(0, 0) as TableLayoutPanel);
		}
		public TableLayoutPanel GetCells(int pageNumber, int cellNumber)
		{
			return (_pagesRowsPanels[pageNumber].GetControlFromPosition(0, cellNumber) as TableLayoutPanel);
		}

		public virtual void Save()
		{
			foreach (KeyValuePair<string, WinFormItem> kvp in _items)
			{
				WinFormItem cdi = kvp.Value;
				if (cdi.LinkedControl == null) { }
				else if (cdi.LinkedControl.GetType() == typeof(CheckBox))
					cdi.Value = ((CheckBox)cdi.LinkedControl).Checked;
				else if (cdi.LinkedControl.GetType() == typeof(ComboBox))
				{
					if (cdi.Mask.ToLower() == "lock")
						cdi.Value = cdi.FirstOrDefault((item)=>item.Caption == cdi.LinkedControl.Text)?.Value;
					else
						cdi.Value = cdi.LinkedControl.Text.ToObjectOf(cdi.DataType);
				}
				else
					cdi.Value = cdi.LinkedControl.Text.ToObjectOf(cdi.DataType);
			}
		}


		public void ShowToolTip(string message, IWin32Window ctrl)
		{
			_errToolTip.Show(message, ctrl, 2000);
		}

		public void ShowToolTip(string message, IWin32Window ctrl, ToolTipIcon icon)
		{
			ShowToolTip(message, ctrl, icon, 2000);
		}

		public void ShowToolTip(string message, IWin32Window ctrl, ToolTipIcon icon, int duration)
		{
			if (_errToolTip.ToolTipIcon != icon)
			{
				ToolTipIcon prevIcon = _errToolTip.ToolTipIcon;
				_errToolTip.ToolTipIcon = icon;
				_errToolTip.Show(message, ctrl, duration);
				_errToolTip.ToolTipIcon = prevIcon;
			}
			else
				_errToolTip.Show(message, ctrl, duration);
		}

		public void SetAutoScrollSize()
		{
			SetAutoScrollSize(0);
		}

		public void SetAutoScrollSize(int page)
		{
			TableLayoutPanel tp = this.GetCells(page);
			(tp.Parent as ScrollableControl).AutoScrollMinSize = new System.Drawing.Size(Math.Min(tp.Width, WinForm.MinFormWidth + 40), tp.Height);
		}

		public void ClearContent()
		{
			this.Items.Clear();
			this._pagesRowsPanels.Clear();
			this.Controls.Clear();
			//this.disabledSTDActions.Clear();
			//this._buttonTablePanel = null;
		}
		#endregion Public

		#region CodeCompletion
		public void AddCodeCompletion(string controlName, string languageID)
		{
			if (CodeCompletionProvider.IsLanguageSupported(languageID))
			{
				CEdit ctrl = GetControl<CEdit>(controlName);
				if (ctrl == null)
					throw new ArgumentException(@"Метод AddCodeCompletion доступен только для елемента управления CEdit, 
елемент управления CEdit с именем: {0} не найден".ToFormat(controlName));
				if (!_codeCompletionInfo.ContainsKey(ctrl.Name))
				{
					_codeCompletionInfo.Add(ctrl.Name, languageID);

					ctrl.TextArea.KeyEventHandler += TextAreaKeyEventHandler;
					ctrl.TextArea.KeyDown += new KeyEventHandler((c, e) => _controlPressed = e.Control);
					ctrl.Disposed += this.CloseCodeCompletionWindow;
					ctrl.LostFocus += this.CloseCodeCompletionWindow;
				}
				else if (!_codeCompletionInfo[ctrl.Name].Equals(languageID, StringComparison.CurrentCultureIgnoreCase))
					_codeCompletionInfo[ctrl.Name] = languageID;
				_quoteChar = CodeCompletionProvider.QuoteChar(languageID);
			}
		}

		private bool TextAreaKeyEventHandler(char key)
		{
			CEdit txtSource = (this.ActiveControl as CEdit);
			if (txtSource == null) return false;

			string langID = _codeCompletionInfo[txtSource.Name];
			if (_codeCompletionWindow != null)
			{
				if (key == '.')
				{
					_codeCompletionWindow.ProcessKeyEvent(key);
				}
				else
					return _codeCompletionWindow.ProcessKeyEvent(key);
			}
			if ((key == '.' || (key == ' ' && _controlPressed) || IsBasicLatin(key)) && !IsQuotedString(txtSource))
			{

				if (txtSource.TextArea.SelectionManager.HasSomethingSelected)
				{
					// allow code completion when overwriting an identifier
					IDocument document = txtSource.TextArea.Document;
					ISelection selection = txtSource.TextArea.SelectionManager.SelectionCollection[0];
					int offset = selection.Offset;
					int endOffset = selection.EndOffset;

					// but block code completion when overwriting only part of an identifier
					if (endOffset < document.TextLength && char.IsLetterOrDigit(document.GetCharAt(endOffset)))
						return false;
					txtSource.TextArea.SelectionManager.RemoveSelectedText();
					txtSource.TextArea.Caret.Position = document.OffsetToPosition(offset);
				}

				CodeCompletionProvider completionDataProvider = new CodeCompletionProvider(langID)
				{
					CtrlEnter = (key == ' ' && _controlPressed),
					LetterStart = IsBasicLatin(key)
				};

				_codeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
					this,                   // The parent window for the completion window
					txtSource,                  // The text editor to show the window for
					txtSource.Name + FileExtension(langID),     // Filename - will be passed back to the provider
					completionDataProvider,     // Provider to get the list of possible completions
					key                         // Key pressed - will be passed to the provider
				);
				if (_codeCompletionWindow != null)
				{
					// ShowCompletionWindow can return null when the provider returns an empty list
					_codeCompletionWindow.Closed += new EventHandler(CloseCodeCompletionWindow);
				}
			}
			return false;
		}

		private string FileExtension(string langID)
		{
			if (langID.Equals("VBNET", StringComparison.CurrentCultureIgnoreCase))
				return ".vb";
			else if (langID.Equals("SQL", StringComparison.CurrentCultureIgnoreCase))
				return ".sql";
			else
				return "";
		}

		private static bool IsBasicLatin(char key)
		{
			return char.IsLetter(key) && ((key >= '\x0041' && key < '\x005A') || (key >= '\x0061' && key < '\x007A'));
		}

		private bool IsQuotedString(CEdit txtSource)
		{
			IDocument doc = txtSource.TextArea.Document;
			LineSegment line = doc.GetLineSegment(doc.GetLineNumberForOffset(txtSource.TextArea.Caret.Offset));
			string str = doc.GetText(line.Offset, txtSource.TextArea.Caret.Position.X);
			int i = str.ToCharArray().Count<char>(chr => chr == _quoteChar);
			return (i % 2 != 0);
		}

		private void CloseCodeCompletionWindow(object sender, EventArgs e)
		{
			if (_codeCompletionWindow != null)
			{
				_codeCompletionWindow.Closed -= new EventHandler(CloseCodeCompletionWindow);
				_codeCompletionWindow.Dispose();
				_codeCompletionWindow = null;
			}
		}
		#endregion CodeCompletion

		#region PrivateMembers
		public static string ShowStringDialog(string source, string language)
		{
			string destination = null;
			//using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			//{
			//	cd.Width = 680;
			//	cd.Height = (int)(cd.Width / vbo.GoldenRatio);
			//	TableLayoutPanel tp = cd.AddCells("WorkTable1", 0, new Size(1, 1), WinFormCellStyle.AutoSize);
			//	tp.Parent.Dock = DockStyle.Fill;
			//	CEdit ce = cd.AddControl<CEdit>("Source", tp);
			//	ce.Dock = DockStyle.Fill;
			//	ce.Text = source;
			//	if (!string.IsNullOrEmpty(language))
			//	{
			//		ce.SetHighlighting(language);
			//		cd.AddCodeCompletion("Source", language);
			//	}
			//	ce.ExitOnEscape = true;
			//	cd.AddStdButtons();
			//	Button btn = cd.AddControl<Button>("Wrap", cd.ButtonTablePanel, new WinFormAddress(0, 0));
			//	btn.Text = "&Перенос строк";
			//	btn.Click += new EventHandler((sender, e) =>
			//	{
			//		int len = Convert.ToInt32(ce.Width / ce.Font.SizeInPoints);
			//		string s = ce.Text.Wrap(len);
			//		ce.Text = s;
			//		ce.Refresh();

			//	});
			//	if (cd.ShowDialog() == DialogResult.OK)
			//		destination = ce.Text;
			//}
			return destination;
		}
		#endregion PrivateMembers

		#region EventHandlers
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Tab | Keys.Control))
			{
				if (_tc != null)
				{
					if (_tc.SelectedIndex == _tc.TabPages.Count - 1)
						_tc.SelectedIndex = 0;
					else
						_tc.SelectedIndex++;
					return true;
				}
			}
			else if (keyData == Keys.Escape && _codeCompletionWindow == null)
			{
				Close();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void WinForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this.GetSTDAction("KeyDown"))
				return;
			if (e.KeyCode == Keys.Escape)
				Close();
		}

		private void Num_KeyPress(object sender, KeyPressEventArgs e)
		{
			TextBoxBase tb = (sender as TextBoxBase);
			if (tb == null) { }
			else if ("-0123456789".IndexOf(e.KeyChar) > -1) { }
			else if ((e.KeyChar == '.' || e.KeyChar == ',') && tb.Text.IndexOf(',') == -1) e.KeyChar = ',';
			else if (e.KeyChar != 8) e.KeyChar = char.MinValue;
		}
		#endregion EventHandlers
	}
}
