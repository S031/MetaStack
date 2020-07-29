using System;
using System.Collections.Generic;
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
using S031.MetaStack.Common;
using TaskPlus.Server.Logging;
using TaskPlus.Server.Middleware;

namespace TaskPlus.Server
{
	public class Startup
	{
		private ILoggerProvider _loggerProvider;
		private ILogger _logger;

		public Startup(IConfiguration configuration)
		{
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRouting();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerProvider loggerProvider)
		{
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			_loggerProvider = loggerProvider; //app.ApplicationServices.GetRequiredService<ILoggerProvider>();
			_logger = _loggerProvider.CreateLogger(Assembly.GetEntryAssembly().GetWorkName());

			app.Use(async (context, next) =>
			{
				context.AddItem<ILogger>(_logger);
				await next();
			});
			app.UseRouter(router =>
			{
				router.DefaultHandler = new RouteHandler(context => new PipeLineController(context).ProcessMessage());
				router.MapRoute("default", "api/{version=v1}/{controller}/{action}");
				router.MapRoute("static_action", "api/{version=v1}/{action}");
			});
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			(_loggerProvider as IDisposable)?.Dispose();
		}
	}
}
