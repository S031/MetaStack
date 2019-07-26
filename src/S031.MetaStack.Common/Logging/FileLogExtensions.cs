namespace S031.MetaStack.Common.Logging
{
	public static class FileLogExtensions
	{
		private static readonly ReadOnlyCache<LogLevels, string> _messages = new ReadOnlyCache<LogLevels, string>(
			(LogLevels.Warning, Properties.Strings.LogLevel_Warning),
			(LogLevels.None, Properties.Strings.LogLevel_None),
			(LogLevels.Information, Properties.Strings.LogLevel_Info),
			(LogLevels.Error, Properties.Strings.LogLevel_Error),
			(LogLevels.Critical, Properties.Strings.LogLevel_Critical),
			(LogLevels.Debug, Properties.Strings.LogLevel_Debug),
			(LogLevels.Trace, Properties.Strings.LogLevel_Trace));

		public static string ToText(this LogLevels level)
			=> _messages[level];
	}
}
