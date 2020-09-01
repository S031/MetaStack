using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using TaskPlus.Server.Actions;
using TaskPlus.Server.Data;
using TaskPlus.Server.Middleware;
using TaskPlus.Server.Security;

namespace TaskPlus.Server
{
	public class Startup
	{
		private ILoggerProvider _loggerProvider;
		//private ILogger _logger;

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRouting();
			services.AddSingleton<ILogger>((svc) => _loggerProvider.CreateLogger(Assembly.GetEntryAssembly().GetWorkName()));
			services.AddSingleton<IMdbContextFactory, MdbContextFactory>();
			services.AddSingleton<IActionManager, ActionManager>();
			services.AddSingleton<ILoginProvider, JwtLoginProvider>();
			services.AddSingleton<IAuthorizationProvider, UserAuthorizationProvider>();
			services.AddSingleton<IUserManager, UserManager>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerProvider loggerProvider)
		{
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			_loggerProvider = loggerProvider; 
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
