using System;
using System.Drawing;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;


namespace S031.MetaStack.WinForms
{
	public static class OutputWindow
	{
		static WinForm _cd;

		static CDialog wind
		{
			get
			{
				if (_cd == null)
				{
					_cd = new CDialog(CDialogPageStyle.SinglePage, CDialogStyle.Dialog);
					_cd.Text = "Окно сообщений";
					TableLayoutPanel tp = _cd.AddCells("WorkTable1", new Size(1, 1));
					tp.Parent.Dock = DockStyle.Fill;
					CEdit editor = _cd.AddControl<CEdit>("LogView", tp);
					editor.SetHighlighting("LOG");
					editor.Dock = DockStyle.Fill;
					_cd.Disposed += (ctrl, e) => _cd = null;
					_cd.ShowInTaskbar = true;
					_cd.Size = FormSettings.Default.OutputWindow_Size;
					_cd.Resize += new EventHandler(cd_Resize);
				}
				return _cd;
			}
		}

		static void cd_Resize(object sender, EventArgs e)
		{
			if (_cd.Visible && _cd.WindowState == FormWindowState.Normal)
			{
				FormSettings.Default.OutputWindow_Size = _cd.Size;
				FormSettings.Default.Save();
			}
		}

		public static void Print(string message)
		{
			CDialog cd = wind;
			CEdit editor = cd.GetControl<CEdit>("LogView");
			TextArea textArea = editor.TextArea;

			textArea.InsertString(message + "\n");

			if (!cd.Visible)
				cd.Visible = true;
		}

		public static void GoEnd()
		{
			CDialog cd = wind;
			CEdit editor = cd.GetControl<CEdit>("LogView");
			IDocument document = editor.Document;
			editor.TextArea.Caret.Position = document.OffsetToPosition(document.TextContent.Length);
		}

		public static void GoTop()
		{
			CDialog cd = wind;
			CEdit editor = cd.GetControl<CEdit>("LogView");
			IDocument document = editor.Document;
			editor.TextArea.Caret.Position = document.OffsetToPosition(0);
		}

		public static void Print(string message, MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Error:
					Print(Logger.SourceTypeError + " " + message);
					break;
				case MessageBoxIcon.Warning:
					Print(Logger.SourceTypeWarning + " " + message);
					break;
				case MessageBoxIcon.Information:
					Print(Logger.SourceTypeInfo + " " + message);
					break;
				default:
					Print(message);
					break;
			}
		}

		public static void Print(string[] messages)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (string item in messages)
				sb.AppendLine(item);
			Print(sb.ToString());
		}

		public static void Focus()
		{
			wind.Focus();
		}

		public static void Clear()
		{
			CDialog cd = wind;
			CEdit editor = cd.GetControl<CEdit>("LogView");
			editor.Text = string.Empty;
		}
	}
}
