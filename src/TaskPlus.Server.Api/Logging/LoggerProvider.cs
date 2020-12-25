using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskPlus.Server.Logging
{
	public abstract class LoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
	{
		private const int default_capacity = 256;
		[ThreadStatic]
		protected static readonly StringBuilder _sb = new StringBuilder(default_capacity);

		private readonly MapTable<string, Logger> _loggers = new MapTable<string, Logger>();
		private IExternalScopeProvider _fScopeProvider;
		protected IDisposable SettingsChangeToken;

		void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
		{
			_fScopeProvider = scopeProvider;
		}

		ILogger ILoggerProvider.CreateLogger(string Category)
		{
			return _loggers.GetOrAdd(Category,
			(category) =>
			{
				return new Logger(this, category);
			});
		}

		void IDisposable.Dispose()
		{
			if (!this.IsDisposed)
			{
				try
				{
					Dispose(true);
				}
				catch
				{
				}

				this.IsDisposed = true;
				GC.SuppressFinalize(this);  // instructs GC not bother to call the destructor   
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (SettingsChangeToken != null)
			{
				SettingsChangeToken.Dispose();
				SettingsChangeToken = null;
			}
		}

		public LoggerProvider()
		{
		}

		~LoggerProvider()
		{
			if (!this.IsDisposed)
			{
				Dispose(false);
			}
		}

		public abstract void WriteLog(LogEntry Info);
		public abstract bool IsEnabled(string category, LogLevel level);

		public Func<LogEntry, string> Formatter { get; set; }
			= (info) =>
			 {
				 string s = "";
				 if (info.Scopes != null && info.Scopes.Count > 0)
				 {
					 LogScopeInfo si = info.Scopes.Last();
					 if (!string.IsNullOrWhiteSpace(si.Text))
						 s = si.Text;
				 }

				 /* writing properties is too much for a text file logger
				 if (Info.StateProperties != null && Info.StateProperties.Count > 0)
				 {
					 Text = Text + " Properties = " + 
							Newtonsoft.Json.JsonConvert.SerializeObject(Info.StateProperties);
				 }                
				 */
				 return string.Join('\t',
					info.TimeStampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"),
					$"[{info.Level}]",
					$"EventId={info.EventId}",
					s,
					info.Text);
			 };


		internal IExternalScopeProvider ScopeProvider
		{
			get
			{
				if (_fScopeProvider == null)
					_fScopeProvider = new LoggerExternalScopeProvider();
				return _fScopeProvider;
			}
		}

		public bool IsDisposed { get; protected set; }
	}
}
