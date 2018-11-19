using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public static class Pipe
	{
		private const int _dueue = 5000;
		private static Timer _timer = null;

		/// <summary>
		/// Start reading from server pipe channal
		/// </summary>
		/// <param name="dueue">period for request server process state default 5 sec</param>
		public static void Start(int dueue = _dueue) =>
			_timer = new Timer(Tick, null, _dueue, _dueue);

		public static void End(bool force = false)
		{
			if (!force)
				Read();

			_timer?.Dispose();
			_timer = null;

		}
		public static bool IsStarted() => _timer != null;

		private static void Tick(object state)
		{
			Read();
		}

		private static void Read()
		{
			try
			{
				var result = ClientGate.Execute("Sys.PipeRead");
				if (result.Read())
					OutputWindow.Print(LogLevels.Trace, ((string)result[0]).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
			}
			catch (Exception ex)
			{
				OutputWindow.Print(LogLevels.Error, $"{ex.Message}\n{ex.StackTrace}");
			}
		}
	}
}
