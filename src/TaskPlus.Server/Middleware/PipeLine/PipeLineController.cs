using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Buffers;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskPlus.Server.Actions;
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

		private static readonly UserInfo _guest= UserManager.GetCurrentPrincipal();


		public PipeLineController(HttpContext context)
		{
			_context = context;
			_config = _context
				.RequestServices
				.GetRequiredService<IConfiguration>();
			_logger = _context
				.RequestServices
				.GetRequiredService<ILogger>();
			_routeValues = _context
				.AddItem<PipeLineRouteValues>(new PipeLineRouteValues(_context))
				.GetItem<PipeLineRouteValues>();
			_actionManager = _context
				.RequestServices
				.GetRequiredService<IActionManager>();
			_userManager = _context
				.RequestServices
				.GetRequiredService<IUserManager>();
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
		public async Task ProcessMessage()
		{
			ActionResult<JsonObject> result;
			ActionInfo ai = await BuildContext();

			if (ai.AsyncMode)
				await _actionManager.ExecuteAsync(ai, DataPackage.Parse(_context.Request.Body.));
				
			//switch (_routeValues.ToString())
			//{
			//	case "default/login":
			//		result = new JwtLoginProvider(_context).Login();
			//		_context.Response.StatusCode = (int)result.StatusCode;
			//		string token = result.Value.ToString(Formatting.Indented);
			//		_logger.LogDebug(token);
			//		await _context.Response.WriteAsync(token);
			//		break;
			//	case "logger/test":
			//		DateTime time = DateTime.Now;
			//		//LoggingSpeedTest();
			//		await ActionManagerCreationSpeedTest();
			//		await _context.Response.WriteAsync($"operation time = {(DateTime.Now - time).TotalMilliseconds}");
			//		await _context.Response.WriteAsync($"API version={_routeValues.Version}, Controller={_routeValues.Controller}, Action={_routeValues.Action}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
			//		break;
			//	default:
			//		result = new JwtLoginProvider(_context).Logon();
			//		_context.Response.StatusCode = (int)result.StatusCode;
			//		await _context.Response.WriteAsync($"API version={_routeValues.Version}, Controller={_routeValues.Controller}, Action={_routeValues.Action}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
			//		break;
			//}
		}

		private async Task<ActionInfo> BuildContext()
		{
			string actionID = (string)_routeValues.Action;
			ActionInfo ai = await _actionManager.GetActionInfoAsync(actionID);
			ActionContext ctx = new ActionContext(_context.RequestServices);
			ctx.CancellationToken = _context.RequestAborted;

			if (ai.AuthenticationRequired)
			{
				var ui = _userManager.GetUserInfo("login");
				ctx.Principal = ui;
			}
			else
				ctx.Principal = _guest;

			ai.SetContext(ctx);
			return ai;
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
