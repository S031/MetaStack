using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlus.Server.Logging.File
{
	static public class FileLoggerExtensions
	{
		public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
		{
			builder.AddConfiguration();

			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider,
				FileLoggerProvider>());
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
				<IConfigureOptions<LoggerFilterOptions>, FileLoggerOptionsSetup>());
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
				<IConfigureOptions<FileLoggerOptions>, FileLoggerOptionsSetup>());
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
				<IOptionsChangeTokenSource<FileLoggerOptions>,
				LoggerProviderOptionsChangeTokenSource<FileLoggerOptions, FileLoggerProvider>>());
			return builder;
		}

		public static ILoggingBuilder AddFile
			   (this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
		{
			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			builder.AddFile();
			builder.Services.Configure(configure);

			return builder;
		}

		public static void LogDebug(this ILogger logger, CallerInfo caller)
			=> logger.Log(LogLevel.Debug, 0, caller, null, null);

		public static HttpContext AddItem<T>(this HttpContext context, T value) where T : class
		{
			var key = typeof(T);
			if (context.Items.ContainsKey(key))
				context.Items[key] = value;
			else
				context.Items.Add(key, value);
			return context;
		}

		public static T GetItem<T>(this HttpContext context)
			=> (T)context.Items[typeof(T)];
	}
}
