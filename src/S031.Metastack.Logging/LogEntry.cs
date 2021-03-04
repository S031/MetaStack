using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Logging
{
	public class LogEntry
	{
		public LogEntry()
		{
		}

		static public readonly string StaticHostName = System.Net.Dns.GetHostName();

		public string UserName { get; } = Environment.UserName;
		public string HostName { get; } = StaticHostName;
		public DateTime TimeStampUtc { get; } = DateTime.UtcNow;
		public string Category { get; set; }
		public LogLevel Level { get; set; }
		public string Text { get; set; }
		public Exception Exception { get; set; }
		public EventId EventId { get; set; }
		public object State { get; set; }
		public string StateText { get; set; }
		public MapTable<string, object> StateProperties { get; set; }
		public List<LogScopeInfo> Scopes { get; set; }
	}
}
