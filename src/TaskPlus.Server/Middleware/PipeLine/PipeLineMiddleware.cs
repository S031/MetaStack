using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPlus.Server.Logging;
using TaskPlus.Server.Middleware;
using TaskPlus.Server.Security.Authentication;

namespace TaskPlus.Server.Middleware
{
	internal class PipeLineController
	{
		private ILogger _logger;
		private PipeLineRouteValues _routeValues;
		private readonly HttpContext _context;

		public PipeLineController(HttpContext context)
		{
			_context = context;
			_logger = _context.GetItem<ILogger>();
			_routeValues = _context
				.AddItem<PipeLineRouteValues>(new PipeLineRouteValues(_context))
				.GetItem<PipeLineRouteValues>();
		}

		public async Task ProcessMessage()
		{
			ActionResult<JsonObject> result;
			switch (_routeValues.ToString())
			{
				case "default-login":
					result = new JwtLoginProvider(_context).Login();
					_context.Response.StatusCode = (int)result.StatusCode;
					await _context.Response.WriteAsync(result.Value.ToString(Formatting.Indented));
					break;
				case "logger-test":
					DateTime time = DateTime.Now;
					LoggingSpeedTest();
					await _context.Response.WriteAsync($"operation time = {(DateTime.Now - time).TotalMilliseconds}");
					await _context.Response.WriteAsync($"API version={_routeValues.Version}, Controller={_routeValues.Controller}, Action={_routeValues.Action}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
					break;
				default:
					result = new JwtLoginProvider(_context).Logon();
					_context.Response.StatusCode = (int)result.StatusCode;
					await _context.Response.WriteAsync($"API version={_routeValues.Version}, Controller={_routeValues.Controller}, Action={_routeValues.Action}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
					break;
			}
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
	}
}
