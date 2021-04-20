using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Caching;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Security;
using TaskPlus.Server.Actions;
using TaskPlus.Server.Api.Settings;
using TaskPlus.Server.Middleware;
using TaskPlus.Server.Security;

namespace TaskPlus.Server
{
	public class Startup
	{
		private ILoggerProvider _loggerProvider;

		/*	AddSingleton() - As the name implies, AddSingleton() method creates a Singleton service. 
		 *		A Singleton service is created when it is first requested. This same instance is then 
		 *		used by all the subsequent requests. So in general, a Singleton service is created only one time 
		 *		per application and that single instance is used throughout the application life time.
		 *	AddTransient() - This method creates a Transient service. A new instance of a Transient 
		 *		service is created each time it is requested.
		 *	AddScoped() - This method creates a Scoped service. A new instance of a Scoped service 
		 *		is created once per request within the scope. For example, in a web application 
		 *		it creates 1 instance per each http request but uses the same instance i
		 *		n the other calls within that same web request.		 
		 *	Never inject Scoped & Transient services into Singleton service. 
		 *		( This effectively converts the transient or scoped service into the singleton.)
		 *	Never inject Transient services into scoped service 
		 *		( This converts the transient service into the scoped. )
		*/

		/// <summary>
		/// Don't use transient for services that uses SQLite connections
		/// </summary>
		/// <param name="services"></param>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRouting();
			services.AddSingleton((svc) => _loggerProvider.CreateLogger(Assembly.GetEntryAssembly().GetWorkName()));
			services.AddSingleton<ILoginProvider, JwtLoginProvider>();
			services.AddSingleton<IAuthorizationProvider, UserAuthorizationProvider>();
			services.AddTransient<ISettingsProvider<VocabularySettingsProvider>, VocabularySettingsProvider>();
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
			ConfigureActions();
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			(_loggerProvider as IDisposable)?.Dispose();
		}

		private static void ConfigureActions()
			=> Actions.ActionsListInternal.CreateActionsList();
	}
}
