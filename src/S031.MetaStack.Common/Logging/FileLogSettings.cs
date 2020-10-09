using System;
using System.IO;

namespace S031.MetaStack.Common.Logging
{
	public class FileLogSettings
    {
		private LogLevels _logLevel = LogLevels.Information;

		public static FileLogSettings Default { get; } = new FileLogSettings(
			basePath: Path.Combine(System.AppContext.BaseDirectory, "Log"),
			cacheSize: 100,
			dateFolderMask: "yyyy-MM",
			filter: null, //(s, l) => l >= LogLevels.Information,
			levelToFlush: LogLevels.Critical,
			formater: (l, s, d) =>
			   $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t[{l.ToString()}]\t{s}\t{d}{Environment.NewLine}"
		)
		{ LogLevel = LogLevels.Information };

		private FileLogSettings(string basePath, int cacheSize, string dateFolderMask,
			Func<string, LogLevels, bool> filter, LogLevels levelToFlush,
			Func<LogLevels, object, object, string> formater)
		{
			BasePath = basePath;
			CacheSize = cacheSize;
			DateFolderMask = dateFolderMask;
			Filter = filter;
			LevelToFlush = levelToFlush;
			Formater = formater;
		}

		public FileLogSettings()
		{
			BasePath = Default.BasePath;
			CacheSize = Default.CacheSize;
			DateFolderMask = Default.DateFolderMask;
			Filter = Default.Filter;
			LevelToFlush = Default.LevelToFlush;
			Formater = Default.Formater;
		}

		/// <summary>
		/// Функция для фильтрации событий в логе
		/// </summary>
		public Func<string, LogLevels, bool> Filter { get; set; }
		
		/// <summary>
		/// Путь до папки Log
		/// </summary>
		public string BasePath{ get; set; }

		/// <summary>
		/// Размер кэша до сброса в файл default 100 (0 - не использовать кэш)
		/// </summary>
		public int CacheSize { get; set; }

		/// <summary>
		/// Установить маску для группировки по датам
		/// Например:
		/// yyyy-MM-dd - каждый день новая папка
		/// yyyy-MM - каждый месяц
		/// </summary>
		public string DateFolderMask { get; set; }

		/// <summary>
		/// Уровень лога (LogLevel) начиная с которого все сообщения сбрасываются из кэша в файл (default LogLevel.Critical)
		/// </summary>
		public LogLevels LevelToFlush { get; set; }

		public Func<LogLevels, object, object, string> Formater { get; set; }

		public LogLevels LogLevel
		{
			get => _logLevel;
			set
			{
				_logLevel = value;
				Filter = (s, l) => l >= _logLevel;
			}
		}
	}
}
