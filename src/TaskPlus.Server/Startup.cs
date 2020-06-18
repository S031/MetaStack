using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
			_logger = _loggerProvider.CreateLogger("TaskPlus.Server");

			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					DateTime time = DateTime.Now;
					for (int i = 0; i < 100000; i++)
						_logger.LogDebug("Debug Hello World!	" + i.ToString());

					await context.Response.WriteAsync($"operation time = {(DateTime.Now - time).TotalMilliseconds}");
				});
			});
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			(_loggerProvider as IDisposable)?.Dispose();
		}
	}
}
