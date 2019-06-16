using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;

namespace S031.MetaStack.Core.Logging
{
	public class FileLogger: FileLog, ILogger
    {
		public FileLogger(string logName) : base(logName, new FileLogSettings())
		{
		}
		public FileLogger(string logName, FileLogSettings settings) : base(logName, settings)
		{
		}
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
				return;

			string source = "";
			if (!eventId.Name.IsEmpty())
				source = $"{eventId.Name}[{eventId.Id}]";

			if (formatter == null)
				formatter = (s, e) =>
				{
					if (s == null && e == null)
						return string.Empty;
					else if (s != null)
						return $"{s}";
					else
						return $"{s}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.StackTrace}";
				};
			Write((LogLevels)logLevel, source, formatter(state, exception));
		}		
		public bool IsEnabled(LogLevel logLevel) => this.Settings.Filter == null ||  this.Settings.Filter(this.LogName, (LogLevels)logLevel);
		public IDisposable BeginScope<TState>(TState state) => new NoopDisposable(this);
		private class NoopDisposable : IDisposable
		{
			readonly FileLogger _fl;
			internal NoopDisposable(FileLogger l)
			{
				_fl = l;
			}
			public void Dispose() => _fl.Flush();
		}
	}
}
