using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using TaskPlus.Server.Security;

namespace TaskPlus.Server.Middleware
{
	internal class PipeLineController
	{
		private readonly PipeLineRouteValues _routeValues;
		private readonly HttpContext _context;
		//private readonly ILogger _logger;
		//private readonly IConfiguration _config;
		//private readonly IUserManager _userManager;
		private readonly IActionManager _actionManager;
		private readonly ILoginProvider _loginProvider;

		private static readonly UserInfo _guest = UserManager.GetCurrentPrincipal();


		public PipeLineController(HttpContext context)
		{
			_context = context;
			var services = _context.RequestServices;
			//_config = services.GetRequiredService<IConfiguration>();
			//_userManager = services.GetRequiredService<IUserManager>();
			//_logger = services.GetRequiredService<ILogger>();
			_actionManager = services.GetRequiredService<IActionManager>();
			_loginProvider = services.GetRequiredService<ILoginProvider>();
			_routeValues = _context
				.AddItem<PipeLineRouteValues>(new PipeLineRouteValues(_context))
				.GetItem<PipeLineRouteValues>();
		}
		public async Task ProcessMessage()
		{
			HttpStatusCode resultCode = HttpStatusCode.OK;
			DataPackage response;
			bool multipleRowsResult = false;

			try
			{
				ActionInfo ai = await BuildContext();
				response = await _actionManager.ExecuteAsync(ai,
					DataPackage.Parse(0, await new StreamReader(_context.Request.Body).ReadToEndAsync()));
				multipleRowsResult = ai.MultipleRowsResult;
			}
			catch (Exception ex)
			{
				response = DataPackage.CreateErrorPackage(ex);
				resultCode = GetCodeFromException(ex);
			}
			_context.Response.StatusCode = (int)resultCode;
			
			if (multipleRowsResult && resultCode == HttpStatusCode.OK)
				await _context.Response.WriteAsync(response.ToString(), Encoding.UTF8);
			else
			{
				response.GoDataTop();
				await _context.Response.WriteAsync(response.GetRowJSON(), Encoding.UTF8);
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
	}
}
