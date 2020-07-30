using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using S031.MetaStack.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace TaskPlus.Server.Security.Authentication
{
	internal class JwtLoginProvider
	{
		private readonly HttpContext _context;

		public JwtLoginProvider(HttpContext context)
		{
			_context = context;
		}

		public ActionResult<JsonObject> Login()
		{
			var response = ActionResult<JsonObject>.Unauthorized();
			var user = AuthenticateUser("F2345@gmail.com", "fackePassword");

			if (user != null)
			{
				var tokenString = GenerateJSONWebToken(user);
				response = ActionResult<JsonObject>.OK(new JsonObject() { ["token"] = tokenString });
			}

			return response;
		}
		public ActionResult<JsonObject> Logon()
		{
			string token = string.Empty;
			if (_context.Request.Headers.TryGetValue("Authorization", out var values))
			{
				string value = values[0];
				if (value.StartsWith("Bearer "))
					token = value.Substring(7);
			}
			if (string.IsNullOrEmpty(token))
				return ActionResult<JsonObject>.Unauthorized();

			_context.User = ValidateToken(token);
			if (_context.User != null)
				return ActionResult<JsonObject>.OK(null);
			return ActionResult<JsonObject>.Unauthorized();
		}


		private static UserModel AuthenticateUser(string login, string password)
			=> new UserModel() { ID = 1, EMail = login, Password = password, Name = "Jonh Smith" };

		private string GenerateJSONWebToken(UserModel user)
		{
			var config = _context.RequestServices.GetService<IConfiguration>()
				.GetSection("Authentication");
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new[] {
				new Claim(JwtRegisteredClaimNames.Sub, user.Name),
				new Claim(JwtRegisteredClaimNames.Email, user.EMail),
				new Claim("DateOfJoing", user.RegisterDate.ToString("yyyy-MM-dd")),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};
			var token = new JwtSecurityToken(config["Jwt:Issuer"],
				config["Jwt:Audience"],				
				claims,
				expires: DateTime.Now.AddMinutes(120),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private ClaimsPrincipal ValidateToken(string jwtToken)
		{
			var config = _context.RequestServices
				.GetService<IConfiguration>()
				.GetSection("Authentication");
			IdentityModelEventSource.ShowPII = true;

			TokenValidationParameters validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidAudience = config["Jwt:Audience"],
				ValidIssuer = config["Jwt:Issuer"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
			};

			ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);
			return principal;
		}
	}
	internal class UserModel
	{
		public int ID { get; set; }
		public string EMail { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public DateTime RegisterDate { get; set; } = DateTime.Now;
	}
	internal class ActionResult<T> where T : class
	{
		public ActionResult(HttpStatusCode statusCode)
		{
			StatusCode = statusCode;
		}

		public HttpStatusCode StatusCode { get; }

		public T Value { get; set; }

		public static ActionResult<T> OK(T value)
			=> new ActionResult<T>(HttpStatusCode.OK) { Value = value };

		public static ActionResult<T> Unauthorized(T value = null)
			=> new ActionResult<T>(HttpStatusCode.Unauthorized) { Value = value };
	}
}
