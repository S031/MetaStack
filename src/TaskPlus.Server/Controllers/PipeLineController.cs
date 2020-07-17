using Microsoft.AspNetCore.Http;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPlus.Server.Security.Authentication;

namespace TaskPlus.Server
{
	public class PipeLineController
	{
		private readonly HttpContext _context;

		public PipeLineController(HttpContext context)
		{
			_context = context;
		}

		public async Task ProcessMessageAsync(string controller, string action)
		{
			var v = _context.Request.RouteValues;
			string ver = (string)v["api_version"];
			ActionResult<JsonObject> result;
			switch ($"{controller}-{action}")
			{
				case "default-login":
					result = new JwtLoginProvider(_context).Login();
					_context.Response.StatusCode = (int)result.StatusCode;
					await _context.Response.WriteAsync(result.Value.ToString(Formatting.Indented));
					break;
				default:
					result = new JwtLoginProvider(_context).Logon();
					_context.Response.StatusCode = (int)result.StatusCode;
					await _context.Response.WriteAsync($"API version={ver}, Controller={controller}, Action={action}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
					break;
			}
		}

		public async Task ProcessMessageAsync(string controller, string action, long id)
		{
			await ProcessMessageAsync(controller, action);
			//var v = _context.Request.RouteValues;
			//string ver = (string)v["api_version"];
			//_context.Response.StatusCode = 200;
			//await _context.Response.WriteAsync($"API version={ver}, Controller={controller}, Action={action}, id={id}, AuthenticationType={_context.User.Identity.AuthenticationType}, Authenticated={_context.User.Identity.IsAuthenticated}");
		}
	}
}
