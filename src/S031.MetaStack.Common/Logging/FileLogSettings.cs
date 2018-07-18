using System;
using System.IO;

namespace S031.MetaStack.Common.Logging
{
	public class FileLogSettings
    {
		private LogLevels _logLevel = LogLevels.Information;
		private static readonly FileLogSettings _default = new FileLogSettings(true);
		public static FileLogSettings Default => _default;
		private FileLogSettings(bool isStatic)
		{
			// Directory.GetCurrentDirectory()
			// Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
			// ASP NET IHostingEnvironment.ApplicationBasePath;
			BasePath = Path.Combine(System.AppContext.BaseDirectory, "Log");
			CacheSize = 100;
			DateFolderMask = "yyyy-MM";
			Filter = (s, l) => l >= LogLevels.Information;
			LevelToFlush = LogLevels.Critical;
			Formater = (l, s, d) => 
				$"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}\t[{l.ToString()}]\t{s}\t{d}{Environment.NewLine}";
		}
		public FileLogSettings()
		{
			BasePath = _default.BasePath;
			CacheSize = _default.CacheSize;
			DateFolderMask = _default.DateFolderMask;
			Filter = _default.Filter;
			LevelToFlush = _default.LevelToFlush;
			Formater = _default.Formater;
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
