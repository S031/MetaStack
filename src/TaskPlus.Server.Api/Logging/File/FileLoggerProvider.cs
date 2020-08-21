using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S031.MetaStack.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskPlus.Server.Logging.File
{
	[Microsoft.Extensions.Logging.ProviderAlias("File")]
	public class FileLoggerProvider : LoggerProvider
	{
		private static readonly MapTable<string, Queue<LogEntry>> _queues =
			new MapTable<string, Queue<LogEntry>>();
		private static readonly object _obj4Lock = new object();

		private readonly string _path;

		public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> Settings)
			: this(Settings.CurrentValue)
		{
			// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens
			SettingsChangeToken = Settings.OnChange(settings =>
			{
				this.Settings = settings;
			});
		}

		public FileLoggerProvider(FileLoggerOptions Settings)
		{
			this.Settings = Settings;
			_path = Settings.DateFolderMask.IsEmpty() ? Settings.BasePath :
				Path.Combine(Settings.BasePath, DateTime.Now.ToString(Settings.DateFolderMask));
			ApplyRetainPolicy();
		}

		internal FileLoggerOptions Settings { get; private set; }

		protected override void Dispose(bool disposing)
		{
			Flush();
			base.Dispose(disposing);
		}

		public override void WriteLog(LogEntry info)
		{
			var queue = _queues.GetOrAdd(info.Category, n => new Queue<LogEntry>());
			lock (_obj4Lock)
				queue.Enqueue(info);
			if (queue.Count > Settings.CacheSize || info.Level >= Settings.LevelToFlush)
				Flush();
		}

		public override bool IsEnabled(string category, LogLevel level)
			=> level != LogLevel.None &&
				(Settings.Filter == null || Settings.Filter(category, level));

		public void Flush()
		{
			foreach (var kvp in _queues)
			{
				var _queue = kvp.Value;
				if (_queue.Count > 0)
				{
					lock (_obj4Lock)
					{
						string filePath = ObtainPath(kvp.Key);
						CheckFileSize(filePath);
						using (FileStream sourceStream = new FileStream(filePath,
							FileMode.Append, FileAccess.Write, FileShare.None,
							bufferSize: 4096, useAsync: false))
						{
							StringBuilder sb = new StringBuilder(1024);
							for (; _queue.TryDequeue(out LogEntry message);)
							{
								sb.AppendLine(Formatter(message));
							}
							byte[] encodedText = Encoding.UTF8.GetBytes(sb.ToString());
							sourceStream.Write(encodedText, 0, encodedText.Length);
						}
					}
				}
			}
		}

		private string ObtainPath(string logName)
		{
			if (!Directory.Exists(_path))
				Directory.CreateDirectory(_path);
			return Path.Combine(_path, logName + ".log");
		}

		void ApplyRetainPolicy()
		{
			FileInfo FI;
			try
			{
				List<FileInfo> FileList = new DirectoryInfo(_path)
				.GetFiles("*.log", SearchOption.TopDirectoryOnly)
				.OrderBy(fi => fi.CreationTime)
				.ToList();

				while (FileList.Count >= Settings.RetainPolicyFileCount)
				{
					FI = FileList.First();
					FI.Delete();
					FileList.Remove(FI);
				}
			}
			catch
			{
			}
		}

		private void CheckFileSize(string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			if (fi.Exists && fi.Length > (1024 * 1024 * Settings.MaxFileSizeInMB))
			{
				string stamp = fi.LastWriteTimeUtc
					.ToLocalTime()
					.ToString("yyyy-MM-dd-HH-mm-ss-fff");
				System.IO.File.Move(fi.FullName, Path.Combine(fi.DirectoryName, $"{fi.Name}-{stamp}{fi.Extension}"));
			}
		}
	}
}
