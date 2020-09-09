using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Threading.Tasks;
using TaskPlus.Server.Logging;
using TaskPlus.Server.Security;

namespace TaskPlus.Server.Middleware
{
	internal class PipeLineController
	{
		private readonly ILogger _logger;
		private readonly PipeLineRouteValues _routeValues;
		private readonly HttpContext _context;
		private readonly IConfiguration _config;
		private readonly IUserManager _userManager;
		private readonly IActionManager _actionManager;
		private readonly ILoginProvider _loginProvider;

		private static readonly UserInfo _guest = UserManager.GetCurrentPrincipal();


		public PipeLineController(HttpContext context)
		{
			_context = context;
			var services = _context.RequestServices;
			_config = services.GetRequiredService<IConfiguration>();
			_logger = services.GetRequiredService<ILogger>();
			_actionManager = services.GetRequiredService<IActionManager>();
			_userManager = services.GetRequiredService<IUserManager>();
			_loginProvider = services.GetRequiredService<ILoginProvider>();
			_routeValues = _context
				.AddItem<PipeLineRouteValues>(new PipeLineRouteValues(_context))
				.GetItem<PipeLineRouteValues>();
		}
		public async Task ProcessMessage()
		{
			HttpStatusCode resultCode = HttpStatusCode.OK;
			DataPackage response = null;
			bool multipleRowsResult = false;

			try
			{
				ActionInfo ai = await BuildContext();
				response = await _actionManager.ExecuteAsync(ai,
					DataPackage.Parse(0, await new StreamReader(_context.Request.Body).ReadToEndAsync()));
				//string data = await new StreamReader(_context.Request.Body).ReadToEndAsync();
				//for (int i = 0; i < 100_000; i++)
				//{
				//	response = await _actionManager.ExecuteAsync(ai,
				//		DataPackage.Parse(0, data));
				//}
				multipleRowsResult = ai.MultipleRowsResult;
			}
			catch (Exception ex)
			{
				response = DataPackage.CreateErrorPackage(ex);
				resultCode = GetCodeFromException(ex);
			}
			_context.Response.StatusCode = (int)resultCode;
			if (multipleRowsResult)
				await _context.Response.WriteAsync(response.ToString());
			else
			{
				response.GoDataTop();
				await _context.Response.WriteAsync(response.GetRowJSON());
			}
		}

		private async Task<ActionInfo> BuildContext()
		{
			string actionID = (string)_routeValues.Action;
			ActionInfo ai = await _actionManager.GetActionInfoAsync(actionID);
			ActionContext ctx = new ActionContext(_context.RequestServices)
			{
				CancellationToken = _context.RequestAborted
			};

			if (ai.AuthenticationRequired)
			{
				var ui = await _loginProvider.LogonAsync(null, null, GetToken());
				ctx.Principal = ui;
			}
			else
				ctx.Principal = _guest;

			ai.SetContext(ctx);
			return ai;
		}

		private string GetToken()
		{
			string token = string.Empty;
			if (_context.Request.Headers.TryGetValue("Authorization", out var values))
			{
				string value = values[0];
				if (value.StartsWith("Bearer "))
					token = value.Substring(7);
			}
			return token;
		}

		/*
			ArgumentException or SerializationException or FormatException is returned as a 400 BadRequest
			NotImplementedException or NotSupportedException is returned as a 405 MethodNotAllowed
			FileNotFoundException is return as 404 NotFound
			AuthenticationException is returned as 401 Unauthorized
			UnauthorizedAccessException is returned as 403 Forbidden
			OptimisticConcurrencyException is returned as 409 Conflict
			All Other normal C# Exceptions are returned as 500 InternalServerError
		 */
		private static readonly ReadOnlyCache<Type, HttpStatusCode> _actionCodes =
			new ReadOnlyCache<Type, HttpStatusCode>
			(
				(typeof(ArgumentException), HttpStatusCode.BadRequest),
				(typeof(SerializationException), HttpStatusCode.BadRequest),
				(typeof(FormatException), HttpStatusCode.BadRequest),
				(typeof(NotImplementedException), HttpStatusCode.MethodNotAllowed),
				(typeof(NotSupportedException), HttpStatusCode.MethodNotAllowed),
				(typeof(FileNotFoundException), HttpStatusCode.NotFound),
				(typeof(AuthenticationException), HttpStatusCode.Unauthorized),
				(typeof(UnauthorizedAccessException), HttpStatusCode.Forbidden),
				(typeof(Exception), HttpStatusCode.InternalServerError)
			);
		private static HttpStatusCode GetCodeFromException(Exception exception)
		{
			if (_actionCodes.TryGetValue(exception.GetType(), out HttpStatusCode code))
				return code;
			return HttpStatusCode.InternalServerError;
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
				}));
			}
			Task.WaitAll(ts.ToArray());
		}

		private async Task ActionManagerCreationSpeedTest()
			=> await Task.Run(() =>
				{
					List<Task> ts = new List<Task>(10);

					for (int j = 0; j < 10; j++)
					{
						ts.Add(Task.Factory.StartNew(() =>
						{
							int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
							IActionManager am;
							for (int i = 1; i <= 100000; i++)
								am = _context.RequestServices.GetRequiredService<IActionManager>(); //new ActionManager(_context.RequestServices);
						}));
					}
					Task.WaitAll(ts.ToArray());
				});


	}
}
