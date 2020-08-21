using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System.Linq;

namespace TaskPlus.Server.Logging.File
{
	internal class FileLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>,
		IConfigureOptions<LoggerFilterOptions>
	{
		private const string provider_alias = "File";
		private static LoggerFilterOptions _loggerFilterOptions;
		private static FileLoggerOptions _loggerFileOptions;

		public FileLoggerOptionsSetup(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
			: base(providerConfiguration.Configuration)
		{
		}

		public override void Configure(FileLoggerOptions options)
		{
			base.Configure(options);
			_loggerFileOptions = options;
			ConfigureInternal();
		}

		public void Configure(LoggerFilterOptions options)
		{
			_loggerFilterOptions = options;
			ConfigureInternal();
		}

		private static void ConfigureInternal()
		{
			if (_loggerFileOptions == null
				|| _loggerFilterOptions == null)
				return;
			_loggerFileOptions.Filter = (category, level) =>
			{
				var rule = _loggerFilterOptions.Rules
					?.Where(r => (string.IsNullOrEmpty(r.ProviderName) || r.ProviderName == provider_alias)
						&& (string.IsNullOrEmpty(r.CategoryName) || r.CategoryName.StartsWith(category, System.StringComparison.OrdinalIgnoreCase)))
					?.OrderBy(r => string.IsNullOrEmpty(r.CategoryName) ? 1024 : r.CategoryName.Length)
					?.FirstOrDefault();

				if (rule == null)
					return true;

				if (rule.Filter != null)
					return rule.Filter(provider_alias, category, level);

				return level >= rule.LogLevel;
			};
		}
	}
}
