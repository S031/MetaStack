using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SA = S031.MetaStack.Common.Properties.Strings;

namespace S031.MetaStack.Common.Logging
{
	public enum LogLevels
	{
		Trace = 0,
		Debug = 1,
		Information = 2,
		Warning = 3,
		Error = 4,
		Critical = 5,
		None = 6
	}
	public class FileLog : IDisposable
	{
		static readonly ConcurrentDictionary<string, Queue<string>> _queues =
			new ConcurrentDictionary<string, Queue<string>>();

		private readonly string _logName;
		private readonly FileLogSettings _settings;
		private readonly Queue<string> _queue;
		private readonly object _lockObj = new object();

		public FileLog(string logName) : this(logName, FileLogSettings.Default)
		{
		}
		public FileLog(string logName, FileLogSettings settings)
		{
			_logName = logName;
			_settings = settings;
			if (!_queues.ContainsKey(_logName))
				_queues[_logName] = new Queue<string>();
			_queue = _queues[_logName];
		}
		public string LogName => _logName;
		public FileLogSettings Settings => _settings;
		public void Write(LogLevels level, string message) => Write(level, "", message);
		public void Write(LogLevels level, string source, string message)
		{
			PutMessage(level, source, message);
			if (_queue.Count > _settings.CacheSize || level >= _settings.LevelToFlush)
				Flush();
		}
		private void PutMessage(LogLevels level, string source, string message)
		{
			if (_settings.Filter(_logName, level))
			{
				if (level == LogLevels.Debug && source.IsEmpty())
					source = Environment.StackTrace.GetToken(2, "\r\n").GetToken(1, " in ").Trim();
				string data = _settings.Formater(level, source, message);
				lock (_lockObj)
					_queue.Enqueue(data);
			}
		}
		public void Debug(object data, [CallerMemberName] string source = "") =>
			Write(LogLevels.Debug, source, $"{data}");

		public void Flush()
		{
			if (_queue.Count > 0)
			{
				string filePath = ObtainPath();
				lock (_lockObj)
				{
					using (FileStream sourceStream = new FileStream(filePath,
						FileMode.Append, FileAccess.Write, FileShare.None,
						bufferSize: 4096, useAsync: false))
					{
#if NETCOREAPP
						StringBuilder sb = new StringBuilder();
						for (; _queue.TryDequeue(out string message);)
						{
							sb.Append(message);
						}
						byte[] encodedText = Encoding.Unicode.GetBytes(sb.ToString());
						sourceStream.Write(encodedText, 0, encodedText.Length);
#else
						for (; _queue.Count > 0;)
						{
							string message = _queue.Dequeue();
							byte[] encodedText = Encoding.Unicode.GetBytes(message);
							sourceStream.Write(encodedText, 0, encodedText.Length);
						}
#endif
					}
				}
			}
		}

		string ObtainPath()
		{
			string path = _settings.DateFolderMask.IsEmpty() ? _settings.BasePath :
				Path.Combine(_settings.BasePath, DateTime.Now.ToString(_settings.DateFolderMask));
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return Path.Combine(path, _logName + ".log");
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Flush();
		}

		/* Not смысла
		public async Task WriteAsync(LogLevels level, string source, string message)
		{
			putMessage(level, source, message);
			if (_queue.Count > _settings.CacheSize || level >= _settings.LevelToFlush)
				await FlushAsync();
		}
		public async Task FlushAsync()
		{
			//await Task.Factory.StartNew(() =>
			//{
			//	flush();
			//});
			if (_queue.Count > 0)
			{
				string filePath = obtainPath();
				{
					using (FileStream sourceStream = new FileStream(filePath,
						FileMode.Append, FileAccess.Write, FileShare.Write,
						bufferSize: 4096, useAsync: false))
					{
#if NETCOREAPP
						string message;
						for (; _queue.TryDequeue(out message);)
						{
							byte[] encodedText = Encoding.Unicode.GetBytes(message);
							await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
						}
#else
							for (; _queue.Count > 0;)
							{
								string message = _queue.Dequeue();
								byte[] encodedText = Encoding.Unicode.GetBytes(message);
								await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
							}
#endif
					}
				}
			}
		}
		*/
	}
}
