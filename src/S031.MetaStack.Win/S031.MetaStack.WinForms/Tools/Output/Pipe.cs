using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace S031.MetaStack.WinForms
{
	public static class Pipe
	{
		private const int _dueue = 3000;
		private static Timer _timer = null;

		public static void Start() =>
			_timer = new Timer(Tick, OutputWindow.GetEditor(), _dueue, _dueue);

		public static void End(bool force = false)
		{
			if (!force)
				Read(OutputWindow.GetEditor());

			_timer?.Dispose();
			_timer = null;

		}
		public static bool IsStarted() => _timer != null;

		private static void Tick(object state)
		{
			Read((CEdit)state);
		}

		private static void Read(CEdit editor)
		{
			var result = ClientGate.Execute("Sys.PipeRead");
			if (result.Read())
			{
				TextArea textArea = editor.TextArea;
				textArea.InsertString((string)result[0] + "\n");
				System.Windows.Forms.Application.DoEvents();
			}
		}
	}
}
