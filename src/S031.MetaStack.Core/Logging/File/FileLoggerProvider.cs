using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;

namespace S031.MetaStack.Core.Logging
{
	public class FileLoggerProvider: ILoggerProvider
    {
		private readonly FileLogSettings _settings;
		private MapTable<string, FileLogger> _loggerList = new MapTable<string, FileLogger>();

		/// <summary>
		/// Initializes a new instance of the <see cref="FileLogLoggerProvider"/> class.
		/// </summary>
		public FileLoggerProvider()
			: this(settings: new FileLogSettings())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileLogLoggerProvider"/> class.
		/// </summary>
		/// <param name="settings">The <see cref="FileLogSettings"/>.</param>
		public FileLoggerProvider(FileLogSettings settings)
		{
			_settings = settings;
		}

		public ILogger CreateLogger(string name)
		{
			if (!_loggerList.ContainsKey(name))
				_loggerList.TryAdd(name, new FileLogger(name, _settings ?? new FileLogSettings()));
            return _loggerList[name];
		}

		public void Dispose()
		{
            foreach (var kvp in _loggerList)
            {
                var loger = kvp.Value;
                if (loger != null)
                    try
                    {
                        loger.Dispose();
                    }
                    catch { }
            }
            //_loggerList = null;
        }

    }
}