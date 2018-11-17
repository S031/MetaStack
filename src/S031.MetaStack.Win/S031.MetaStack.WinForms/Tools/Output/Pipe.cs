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
		private const int _dueue = 1000;
		private static Timer _timer = null;

		public static void Start() =>
			_timer = new Timer(Tick, OutputWindow.GetDataSource(), _dueue, _dueue);

		public static void End(bool force = false)
		{
			if (!force)
				Read(OutputWindow.GetDataSource());

			_timer?.Dispose();
			_timer = null;

		}
		public static bool IsStarted() => _timer != null;

		private static void Tick(object state)
		{
			Read((DataTable)state);
		}

		private static void Read(DataTable dt)
		{
			var result = ClientGate.Execute("Sys.PipeRead");
			if (result.Read())
			{
				foreach (string messsage in ((string)result[0]).Split("\r\n".ToCharArray()))
					if (!messsage.IsEmpty())
						//dt.Rows.Add(DateTime.Now, LogLevels.Information, messsage);
						OutputWindow.Print(LogLevels.Information, messsage);
			}
		}
	}
}
