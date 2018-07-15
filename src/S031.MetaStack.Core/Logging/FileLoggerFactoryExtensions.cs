using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Logging
{
	public static class FileLoggerExtensions
	{
		/// <summary>
		/// Adds an File logger that is enabled for <see cref="LogLevel"/>.Information or higher.
		/// </summary
		/// <param name="factory">The extension method argument.</param>
		public static ILoggerFactory AddFileLog(this ILoggerFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return AddFileLog(factory, Microsoft.Extensions.Logging.LogLevel.Information);
		}

		/// <summary>
		/// Adds an File logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
		/// </summary>
		/// <param name="factory">The extension method argument.</param>
		/// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
		public static ILoggerFactory AddFileLog(this ILoggerFactory factory, Microsoft.Extensions.Logging.LogLevel minLevel)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return AddFileLog(factory, new FileLogSettings()
			{
				Filter = (_, logLevel) => (Microsoft.Extensions.Logging.LogLevel)logLevel >= minLevel
			});
		}

		/// <summary>
		/// Adds an File logger. Use <paramref name="settings"/> to enable logging for specific <see cref="LogLevel"/>s.
		/// </summary>
		/// <param name="factory">The extension method argument.</param>
		/// <param name="settings">The <see cref="FileLogSettings"/>.</param>
		public static ILoggerFactory AddFileLog(
			this ILoggerFactory factory,
			FileLogSettings settings)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			factory.AddProvider(new FileLoggerProvider(settings));
			return factory;
		}
		/// <summary>
		/// Adds an File logger. Use <paramref name="settings"/> to enable logging for specific <see cref="LogLevel"/>s.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		/// <param name="settings">The <see cref="FileLogSettings"/>.</param>
		/// <returns></returns>
		public static ILoggingBuilder AddFileLog(this ILoggingBuilder builder, FileLogSettings settings = null)
		{
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));

			if (settings == null)
				settings = FileLogSettings.Default;

			builder.AddProvider(new FileLoggerProvider(settings));
			return builder;
		}
		public static void Debug(this ILogger logger, string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
		{
			logger.LogDebug(new EventId(0, callerName), message);
		}
	}
}
