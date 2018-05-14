using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
	public class CEdit: TextEditorControl
	{
		private ContextMenuStrip mnuEdit = new ContextMenuStrip();

		WinForm _cd;
		private static List<string> _searches = new List<string>();
		private static List<string> _replaces = new List<string>();
		static StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;
		static string _searchText = string.Empty;
		static string _replaceText = string.Empty;
		int _lineNum = 1;
		int _startIndex;

		public CEdit()
		{
			this.BorderStyle = BorderStyle.FixedSingle;
			this.ShowLineNumbers = false;
			this.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Smart;
			ExitOnEscape = false;
			mnuEdit.Items.Add(new ToolStripMenuItem("Вырезать", ResourceManager.GetImage("Cut"), new EventHandler(contextMenuClick) ) { Name = "mnuEditCut" });
			mnuEdit.Items.Add(new ToolStripMenuItem("Копировать", ResourceManager.GetImage("Copy"), new EventHandler(contextMenuClick) ) { Name = "mnuEditCopy" });
			mnuEdit.Items.Add(new ToolStripMenuItem("Вставить", ResourceManager.GetImage("Paste"), new EventHandler(contextMenuClick) ) { Name = "mnuEditPaste" });
			mnuEdit.Items.Add(new ToolStripSeparator());
			mnuEdit.Items.Add(new ToolStripMenuItem("Найти...", ResourceManager.GetImage("Find"), new EventHandler(contextMenuClick)) { Name = "mnuEditFind", ShortcutKeys = Keys.Control | Keys.F});
			mnuEdit.Items.Add(new ToolStripMenuItem("Заменить...", null, new EventHandler(contextMenuClick) ) { Name = "mnuEditFindReplace", ShortcutKeys = Keys.Control | Keys.H });
			mnuEdit.Items.Add(new ToolStripMenuItem("Перейти...", null, new EventHandler(contextMenuClick) ) { Name = "mnuEditGoTo", ShortcutKeys = Keys.Control | Keys.G });
			mnuEdit.Items.Add(new ToolStripSeparator());
			mnuEdit.Items.Add(new ToolStripMenuItem("Отменить", ResourceManager.GetImage("Undo"), new EventHandler(contextMenuClick)) { Name = "mnuEditUndo", Enabled = false});
			mnuEdit.Items.Add(new ToolStripMenuItem("Вернуть", ResourceManager.GetImage("Redo"), new EventHandler(contextMenuClick)) { Name = "mnuEditRedo", Enabled = false});
			mnuEdit.Items.Add(new ToolStripSeparator());
			mnuEdit.Items.Add(new ToolStripMenuItem("Формат") { Name = "mnuFormat"});
			((ToolStripMenuItem)mnuEdit.Items["mnuFormat"]).DropDownItems.AddRange(new ToolStripItem[] {
				new ToolStripMenuItem("Верхний регистр", null, contextMenuClick, Keys.Control|Keys.U) { Name = "mnuFormatToUpper" },
				new ToolStripMenuItem("Нижний регистр", null, contextMenuClick, Keys.Control|Keys.L) { Name = "mnuFormatToLower" },
				new ToolStripMenuItem("Инвертировать регистр", null, contextMenuClick) { Name = "mnuFormatInvert" },
				new ToolStripMenuItem("JSON формат", null, contextMenuClick) { Name = "mnuFormatJSON" },
				new ToolStripMenuItem("XML формат", null, contextMenuClick) { Name = "mnuFormatXML" },
				new ToolStripMenuItem("Space2Tab", null, contextMenuClick) { Name = "mnuFormatSpace2Tab" }});
			FontManager.Adop(mnuEdit);
			this.ContextMenuStrip = mnuEdit;
			this.TextArea.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CEdit_KeyDown);
			this.Disposed += new EventHandler((o, e) => { if (_cd != null) _cd.Dispose(); });
			this.TextChanged += new EventHandler((ctrl, e) => UndoControl());
			this.Document.UndoStack.ActionUndone += new EventHandler((ctrl, e) => UndoControl());
			this.Document.UndoStack.ActionRedone += new EventHandler((ctrl, e) => UndoControl());
			this.TextArea.SelectionManager.SelectionChanged+=new EventHandler((ctrl, e) => selectionControl());
			selectionControl();
		}
		
		public bool ExitOnEscape { get; set; }

		public TextArea TextArea
		{
			get { return this.ActiveTextAreaControl.TextArea; }
		}

		public void GoTo(int lineNumber)
		{
			if (lineNumber > 0 && lineNumber < this.Document.TotalNumberOfLines)
			{
				var ctrl = this.TextArea;
				ctrl.SelectionManager.ClearSelection();
				ctrl.Caret.Position = new ICSharpCode.TextEditor.TextLocation(0, lineNumber - 1);
				
				var tview = ctrl.TextView;
				int vcount = lineNumber - tview.VisibleLineCount / 2 + 1;
				if (vcount > 0)
					tview.FirstVisibleLine = vcount;
			}
		}

		public void ReplaceSelection(string textForReplace)
		{
			IDocument document = this.TextArea.Document;
			int offset;
			int lenght;
			if (this.TextArea.SelectionManager.HasSomethingSelected)
			{
				ISelection selection = this.TextArea.SelectionManager.SelectionCollection[0];
				offset = selection.Offset;
				lenght = selection.SelectedText.Length;
			}
			else
			{
				offset = this.TextArea.Caret.Offset;
				lenght = 0;
			}

			this.TextArea.BeginUpdate();
			this.TextArea.SelectionManager.ClearSelection();
			document.Replace(offset, lenght, textForReplace);
			this.TextArea.Caret.Position = document.OffsetToPosition(offset + textForReplace.Length);
			this.TextArea.EndUpdate();
		}

		void selectionControl()
		{
			bool canCopy = this.TextArea.SelectionManager.HasSomethingSelected; ;

			mnuEdit.Items["mnuEditCut"].Enabled = canCopy;
			mnuEdit.Items["mnuEditCopy"].Enabled = canCopy;
			mnuEdit.Items["mnuEditPaste"].Enabled = Clipboard.ContainsText();
		}

		void UndoControl()
		{
			mnuEdit.Items["mnuEditUndo"].Enabled = this.EnableUndo;
			mnuEdit.Items["mnuEditRedo"].Enabled = this.EnableRedo;
		}

		#region Find&Replace Support
		public void Find()
		{
			if (_cd != null) _cd.Dispose();
			_cd = new WinForm(WinFormStyle.Dialog);
			_cd.Size = new Size(350, Convert.ToInt32(350 / vbo.GoldenRatio));
			_cd.Text = "Найти...";

			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			TableLayoutPanel tlpRows = _cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 2) });
			p.ColumnStyles[0].SizeType = SizeType.Percent;
			p.ColumnStyles[0].Width = 100;
			p.ColumnStyles[1].SizeType = SizeType.Absolute;
			p.ColumnStyles[1].Width = 100;
			ComboBox cbox = p.Add<ComboBox>(new WinFormItem("SearchText")
			{
				PresentationType = typeof(ComboBox),
				DataType = typeof(string),
				Value = GetInitialSearchText()
			});
			cbox.Items.AddRange(_searches.ToArray());
			setAutoComplete(cbox);
			_cd.Activated += new EventHandler((o, e) => { ((WinForm)o).GetControl<ComboBox>("SearchText").Focus(); });
			Button btn = p.Add<Button>("Find");
			btn.Text = "Найти";
			btn.Click += new EventHandler(btn_Click);
			_cd.AcceptButton = btn;
			CheckBox cb = p.Add<CheckBox>(new WinFormItem("MatchCase") { Value = (_stringComparison != StringComparison.InvariantCultureIgnoreCase), DataType = typeof(bool), PresentationType = typeof(CheckBox) });
			cb.Text = "С учетом регистра";
			tlpRows.Add<Label>("Blank");
			_cd.Show(this);
		}
		
		public void Replace()
		{
			//Object[] values = InputBox.Show(new WinFormItem("DateOper") { Caption = "Дата операции", DataType = typeof(DateTime), Value = vbo.Date() },
			//	new WinFormItem("Amount") { Caption = "Сумма", DataType = typeof(decimal), Value = 1000000, Format = vbo.CurrencyFormat });

			if (_cd != null) _cd.Dispose();
			_cd = new WinForm(WinFormStyle.Dialog);
			_cd.Size = new Size(350, Convert.ToInt32(350 / vbo.GoldenRatio));
			_cd.Text = "Найти...";

			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			TableLayoutPanel tlpRows = _cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 3) });
			p.ColumnStyles[0].SizeType = SizeType.Percent;
			p.ColumnStyles[0].Width = 100;
			p.ColumnStyles[1].SizeType = SizeType.Absolute;
			p.ColumnStyles[1].Width = 100;

			ComboBox cbox = p.Add<ComboBox>(new WinFormItem("SearchText")
			{
				PresentationType = typeof(ComboBox),
				DataType = typeof(string),
				Value = GetInitialSearchText(),
				CellAddress = new Pair<int>(0, 0)
			});
			cbox.Items.AddRange(_searches.ToArray());
			setAutoComplete(cbox);
			_cd.Activated += new EventHandler((o, e) => { ((WinForm)o).GetControl<ComboBox>("SearchText").Focus(); });

			cbox = p.Add<ComboBox>(new WinFormItem("ReplaceText")
			{
				PresentationType = typeof(ComboBox),
				DataType = typeof(string),
				Value = _replaceText,
				CellAddress = new Pair<int>(0, 1)
			});
			cbox.Items.AddRange(_replaces.ToArray());
			setAutoComplete(cbox);

			CheckBox cb = p.Add<CheckBox>(new WinFormItem("MatchCase")
			{
				Value = (_stringComparison != StringComparison.InvariantCultureIgnoreCase),
				DataType = typeof(bool),
				PresentationType = typeof(CheckBox),
				CellAddress = new Pair<int>(0, 2)
			});
			cb.Text = "С учетом регистра";

			Button btn = p.Add<Button>("Find");
			btn.Text = "Найти";
			btn.Click += new EventHandler(btn_Click);
			_cd.AcceptButton = btn;

			btn = p.Add<Button>("Replace");
			btn.Text = "Заменить";
			btn.Click += new EventHandler(btn_Click);
			
			btn = p.Add<Button>("ReplaceAll");
			btn.Text = "Заменить все";
			btn.Click += new EventHandler(btn_Click);
			tlpRows.Add<Label>("Blank");

			_cd.Show(this);
		}

		void btn_Click(object sender, EventArgs e)
		{
			_cd.Save();
			string searchText = (string)_cd.GetItem("SearchText").Value;
			Button btn = (sender as Button);
			if (btn.GetItem().Name == "Find")
			{
				int offset = this.TextArea.Document.PositionToOffset(this.TextArea.Caret.Position);
				_startIndex = offset;
				bool withStore = (_searchText != searchText);
				_searchText = searchText;
				_stringComparison = (bool)_cd.GetItem("MatchCase").Value ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
				runSearch(withStore);
			}
			else if (btn.GetItem().Name == "Replace")
			{
				_stringComparison = (bool)_cd.GetItem("MatchCase").Value ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
				_replaceText = (string)_cd.GetItem("ReplaceText").Value;
				_searchText = searchText;
				if (!string.IsNullOrEmpty(searchText))
				{
					runReplace();
					runSearch(false);
					storeAutoComplete(true);
				}
			}
			else if (btn.GetItem().Name == "ReplaceAll")
			{
				_stringComparison = (bool)_cd.GetItem("MatchCase").Value ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
				_replaceText = (string)_cd.GetItem("ReplaceText").Value;
				_searchText = searchText;
				if (!string.IsNullOrEmpty(searchText))
				{
					runReplaceAll();
					storeAutoComplete(true);
				}
			}
		}

		void runReplace()
		{
			IDocument document = this.TextArea.Document;
			int offset;
			int lenght;
			if (this.TextArea.SelectionManager.HasSomethingSelected)
			{
				ISelection selection = this.TextArea.SelectionManager.SelectionCollection[0];
				offset = selection.Offset;
				lenght = selection.SelectedText.Length;
			}
			else
				return;

			this.TextArea.BeginUpdate();
			this.TextArea.SelectionManager.ClearSelection();
			document.Replace(offset, lenght, _replaceText);
			this.TextArea.Caret.Position = document.OffsetToPosition(offset + _replaceText.Length);
			this.TextArea.EndUpdate();
			cdShowPosition(this.TextArea.Caret.Position);
		}

		void runReplaceAll()
		{
			IDocument document = this.TextArea.Document;
			int offset;
			int lenght;
			string sourceText;
			if (this.TextArea.SelectionManager.HasSomethingSelected)
			{
				ISelection selection = this.TextArea.SelectionManager.SelectionCollection[0];
				lenght = selection.SelectedText.Length;
				if (lenght > _searchText.Length)
				{
					offset = selection.Offset;
					sourceText = selection.SelectedText;
				}
				else
				{
					offset = 0;
					lenght = this.Document.TextContent.Length;
					sourceText = this.Document.TextContent;
				}
			}
			else
			{
				offset = 0;
				lenght = this.Document.TextContent.Length;
				sourceText = this.Document.TextContent;
			}
			if (_stringComparison == StringComparison.InvariantCultureIgnoreCase)
				sourceText = System.Text.RegularExpressions.Regex.Replace(sourceText, _searchText, _replaceText, 
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			else
				sourceText = sourceText.Replace(_searchText, _replaceText);

			this.TextArea.BeginUpdate();
			this.TextArea.SelectionManager.ClearSelection();
			document.Replace(offset, lenght, sourceText);
			this.TextArea.Caret.Position = document.OffsetToPosition(offset + _replaceText.Length);
			this.TextArea.EndUpdate();
			cdShowPosition(this.TextArea.Caret.Position);
		}

		void runSearch(bool withStore)
		{
			string find = _searchText;
			if (_startIndex > this.Document.TextContent.Length) _startIndex = 0;
			int index = this.Document.TextContent.IndexOf(find, _startIndex, _stringComparison);
			if (index == -1)
			{
				index = this.Document.TextContent.IndexOf(find, 0, _stringComparison);;
			}

			if (index >= 0)
			{
				_startIndex = index;
				HighlightText(index, find.Length);
				cdShowPosition(this.TextArea.Caret.Position);
			}
			else
				cdShowPosition(TextLocation.Empty);

			if (withStore)
				storeAutoComplete(false);
		}

		void cdShowPosition(TextLocation position)
		{
			if (_cd != null && position != TextLocation.Empty)
				_cd.GetItem("Blank").LinkedControl.Text = $"{position}";
			else if (_cd != null)
				_cd.GetItem("Blank").LinkedControl.Text = "";
		}
		void HighlightText(int offset, int length)
		{
			int endOffset = offset + length;
			this.TextArea.Caret.Position = this.TextArea.Document.OffsetToPosition(endOffset);
			this.TextArea.SelectionManager.ClearSelection();
			IDocument document = this.TextArea.Document;
			DefaultSelection selection = new DefaultSelection(document, document.OffsetToPosition(offset), document.OffsetToPosition(endOffset));
			this.TextArea.SelectionManager.SetSelection(selection);
			this.Refresh();
		}

		private string GetInitialSearchText()
		{
			if (this.TextArea.SelectionManager.HasSomethingSelected)
			{
				ISelection selection = this.TextArea.SelectionManager.SelectionCollection[0];
				_startIndex = selection.Offset;
				//if (selection.StartPosition.Y == selection.EndPosition.Y)
				//{
					return selection.SelectedText;
				//}
			}
			return _searchText;
		}

		private static void setAutoComplete(ComboBox tb)
		{
			tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			tb.AutoCompleteSource = AutoCompleteSource.ListItems;
		}

		private static void storeAutoComplete(bool withReplace)
		{
			if (_searches.Contains(_searchText)) _searches.Remove(_searchText);
			_searches.Insert(0, _searchText);
			if (withReplace)
			{
				if (_replaces.Contains(_replaceText)) _replaces.Remove(_replaceText);
				_replaces.Insert(0, _replaceText);
			}
		}
		#endregion Find&Replace Support

		#region EventImplements
		private void contextMenuClick(object sender, EventArgs e)
		{
			switch ((sender as ToolStripMenuItem).Name)
			{
				case "mnuEditCut":
					new Cut().Execute(this.TextArea);
					break;
				case "mnuEditCopy":
					new Copy().Execute(this.TextArea);
					break;
				case "mnuEditPaste":
					new Paste().Execute(this.TextArea);
					break;
				case "mnuEditFind":
					Find();
					break;
				case "mnuEditFindReplace":
					Replace();
					break;
				case "mnuEditUndo":
					this.Undo();
					break;
				case "mnuEditRedo":
					this.Redo();
					break;
				case "mnuFormatToUpper":
					new ToUpperCase().Execute(this.TextArea);
					break;
				case "mnuFormatToLower":
					new ToLowerCase().Execute(this.TextArea);
					break;
				case "mnuFormatInvert":
					new InvertCaseAction().Execute(this.TextArea);
					break;
				case "mnuFormatJSON":
					new ToJSON().Execute(this.TextArea);
					break;
				case "mnuFormatSpace2Tab":
					new ConvertLeadingSpacesToTabs().Execute(this.TextArea);
					break;
				case "mnuEditGoTo":
					int oldLine = _lineNum;
					if ((_lineNum = InputBox.Show<int>("Номер строки", _lineNum, -1)) != -1)
						GoTo(_lineNum);
					else
						_lineNum = oldLine;
					break;

			}
		}
		
		void CEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.F3)
			{
				_searchText = GetInitialSearchText();
				_startIndex += 1;
				runSearch(true);
			}
			else if (e.KeyCode == Keys.F3)
			{
				int offset = this.TextArea.Document.PositionToOffset(this.TextArea.Caret.Position);
				_startIndex = offset;
				runSearch(false);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			/*if (keyData == Keys.Escape && ExitOnEscape)
			{
				this.ParentForm.Close();
				return true;
			}
			else*/ if (keyData == (Keys.Enter | Keys.Control) )
			{
				if (this.ParentForm.AcceptButton != null)
					this.ParentForm.AcceptButton.PerformClick();
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		class ToJSON : AbstractSelectionFormatAction
		{
			protected override void Convert(IDocument document, int startOffset, int length)
			{
                string what = document.GetText(startOffset, length);
				try
				{
                    Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(what);
                    what = json.ToString(Newtonsoft.Json.Formatting.Indented);
                    document.Replace(startOffset, length, what);
                }
				catch
				{
				}
			}
		}
		#endregion EventImplements
	}
}
