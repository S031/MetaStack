using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S031.MetaStack.Common;
using TaskPlus.Server.Logging;
using TaskPlus.Server.Logging.File;

namespace TaskPlus.Server
{
	public class Startup
	{
		private readonly IConfiguration _configuration;
		private ILoggerProvider _loggerProvider;
		private ILogger _logger;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			
			_loggerProvider = app.ApplicationServices.GetRequiredService<ILoggerProvider>();
			_logger = _loggerProvider.CreateLogger(Assembly.GetEntryAssembly().GetWorkName());

			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			app.UseRouting();
			app.UseEndpoints(endpoints => ConfigureEndpoints(endpoints));
		}

		private void LoggingSpeedTest()
		{
			List<Task> ts = new List<Task>(10);

			for (int j = 0; j < 10; j++)
			{
				ts.Add(Task.Factory.StartNew(() =>
				{
					int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
			for (int i = 1; i <= 100000; i++)
				_logger.LogDebug(new CallerInfo($"Сообщение № {i} в потоке {id}"));
					//logger.LogInformation(0, new CallerInfo($"Сообщение № {i} в потоке {id}").ToString());
					//_logger.Log<TaskPlus.Server.Startup>(LogLevel.Debug, 0, this, null, null);
				}));
			}
			Task.WaitAll(ts.ToArray());

		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			(_loggerProvider as IDisposable)?.Dispose();
		}

		private void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
		{
			endpoints.MapGet("/", async context =>
			{
				DateTime time = DateTime.Now;
				LoggingSpeedTest();
				await context.Response.WriteAsync($"operation time = {(DateTime.Now - time).TotalMilliseconds}");
			});

		}
	}
}
