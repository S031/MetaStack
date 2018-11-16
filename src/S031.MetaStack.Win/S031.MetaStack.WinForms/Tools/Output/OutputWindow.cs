using System;
using System.Drawing;
using System.Windows.Forms;
using S031.MetaStack.Common.Logging;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;


namespace S031.MetaStack.WinForms
{
	public static class OutputWindow
	{
		private static WinForm _cd;
		private static CEdit _editor;

		static OutputWindow()
		{
			_cd = new WinForm(WinFormStyle.Dialog);
			_cd.Text = "Окно сообщений";
			_cd.Add<Panel>(WinFormConfig.SinglePageForm);
			_cd.FormClosing += _cd_FormClosing;
			TableLayoutPanel tlpRows = _cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			tlpRows.Add(new WinFormItem($"LogView")
			{
				PresentationType = typeof(CEdit),
				ControlTrigger = (cdi, ctrl) =>
				{
					_editor = (ctrl as CEdit);
					_editor.SetHighlighting("LOG");
					_editor.Dock = DockStyle.Fill;
				}
			});
		}

		private static void _cd_FormClosing(object sender, FormClosingEventArgs e)
		{
			Clear();
			_cd.Visible = false;
			e.Cancel = true;
		}

		static void cd_Resize(object sender, EventArgs e)
		{
			if (_cd.Visible && _cd.WindowState == FormWindowState.Normal)
			{
				//FormSettings.Default.OutputWindow_Size = _cd.Size;
				//FormSettings.Default.Save();
			}
		}

		public static void Print(string message)
		{
			TextArea textArea = _editor.TextArea;
			textArea.InsertString(message + "\n");

			if (!_cd.Visible)
				_cd.Visible = true;
		}

		public static void GoEnd()
		{
			IDocument document = _editor.Document;
			_editor.TextArea.Caret.Position = document.OffsetToPosition(document.TextContent.Length);
		}

		public static void GoTop()
		{
			IDocument document = _editor.Document;
			_editor.TextArea.Caret.Position = document.OffsetToPosition(0);
		}

		public static void Print(LogLevels level, string message) =>
			Print($"{FileLog.Messages[level]} {message}");

		public static void Print(string[] messages)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (string item in messages)
				sb.AppendLine(item);
			Print(sb.ToString());
		}

		public static void Focus()
		{
			_cd.Focus();
		}

		public static void Clear()
		{
			_editor.Text = string.Empty;
		}
		public static CEdit GetEditor() => _editor;
	}
}
