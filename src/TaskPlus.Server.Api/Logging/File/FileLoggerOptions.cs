using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlus.Server.Logging.File
{
    public class FileLoggerOptions
    {
		int _fMaxFileSizeInMB;
		int _fRetainPolicyFileCount;

        public FileLoggerOptions() { }

        public string Folder => BasePath;

        public int MaxFileSizeInMB
        {
            get { return _fMaxFileSizeInMB > 0 ? _fMaxFileSizeInMB : 20; }
            set { _fMaxFileSizeInMB = value; }
        }

        public int RetainPolicyFileCount
        {
            get { return _fRetainPolicyFileCount < 256 ? 256 : _fRetainPolicyFileCount; }
            set { _fRetainPolicyFileCount = value; }
        }

        /// <summary>
        /// Путь до папки Log
        /// </summary>
        public string BasePath { get; set; } = Path.Combine(System.AppContext.BaseDirectory, "Log");

        /// <summary>
        /// Размер кэша до сброса в файл default 100 (0 - не использовать кэш)
        /// </summary>
        public int CacheSize { get; set; } = 100;

        /// <summary>
        /// Установить маску для группировки по датам
        /// Например:
        /// yyyy-MM-dd - каждый день новая папка
        /// yyyy-MM - каждый месяц
        /// </summary>
        public string DateFolderMask { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Уровень лога (LogLevel) начиная с которого все сообщения сбрасываются из кэша в файл (default LogLevel.Critical)
        /// </summary>
        public LogLevel LevelToFlush { get; set; } = LogLevel.Critical;

        public Func<string, LogLevel, bool> Filter { get; set; }
    }
}
