using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Json;
using S031.MetaStack.Security;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TaskPlus.Server.Actions;
using TaskPlus.Server.Api.Properties;
using TaskPlus.Server.Data;

namespace TaskPlus.Server.Security
{
	public class JwtLoginProvider : ILoginProvider
	{
		private readonly IServiceProvider _services;
		private readonly IConfiguration _config;
		private readonly IUserManager _um;
		private readonly ILogger _logger;

		private readonly IConfigurationSection _cs;
		private readonly SymmetricSecurityKey _securityKey;
		private readonly SigningCredentials _credentials;


		public JwtLoginProvider(IServiceProvider services)
		{
			_services = services;
			_config = services.GetRequiredService<IConfiguration>();
			_logger = services.GetRequiredService<ILogger>();
			_um = services.GetRequiredService<IUserManager>();

			_cs = _config.GetSection("Authentication");
			_securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cs["jwt:Key"]));
			_credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
		}

		/// <summary>
		/// Authenticate and create JWT token for user
		/// </summary>
		/// <param name="userName">LOgin user name or emaile</param>
		/// <param name="clientLoginData">user password</param>
		/// <returns></returns>
		public string LoginRequest(string userName, string clientLoginData)
			=> new JwtSecurityTokenHandler()
			.WriteToken(
				GenerateToken(
					AuthenticateUser(userName, clientLoginData)));

		/// <summary>
		/// Authenticate and create JWT token for user async
		/// </summary>
		/// <param name="userName">LOgin user name or emaile</param>
		/// <param name="clientLoginData">user password</param>
		/// <returns></returns>
		public async Task<string> LoginRequestAsync(string userName, string clientLoginData)
			=> new JwtSecurityTokenHandler()
			.WriteToken(
				GenerateToken(
					await AuthenticateUserAsync(userName, clientLoginData)));

		public UserInfo Logon(string userName, string sessionID, string encryptedKey)
		{
			throw new NotImplementedException();
		}

		public async Task<UserInfo> LogonAsync(string userName, string sessionID, string encryptedKey)
		{
			var login = ValidateToken(encryptedKey);
			return await _um.GetUserInfoAsync(login);
		}

		public void Logout(string userName, string sessionID, string encryptedKey)
		{
			throw new NotImplementedException();
		}

		public Task LogoutAsync(string userName, string sessionID, string encryptedKey)
		{
			throw new NotImplementedException();
		}

		UserInfo AuthenticateUser(string userName, string password)
			=> AuthenticateUserAsync(userName, password)
			.GetAwaiter()
			.GetResult();

		private async Task<UserInfo> AuthenticateUserAsync(string userName, string password)
		{
			// Find in catalog (not found error)
			var ui = await _um.GetUserInfoAsync(userName);
			if (ui == null)
				throw new AuthenticationException($"Login for name '{userName}' not registered in system catalog");
			
			string impersonateType = ui.Identity.AuthenticationType;
			if (impersonateType.Equals("windows", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					Impersonator.Execute<bool>(userName, password, () => true);
				}
				catch (Exception ex)
				{
					throw new AuthenticationException($"Authentication failed for user '{userName}'", ex);
				}
			}
			else //basic
			{
				if (!ui.PasswordHash.Equals(CryptoHelper.ComputeSha256Hash(password), StringComparison.Ordinal))
					throw new AuthenticationException($"Bad password for user '{userName}'");
			}
			return ui;
		}

		private JwtSecurityToken GenerateToken(UserInfo principal)
		{
			return new JwtSecurityToken(_cs["Jwt:Issuer"],
				_cs["Jwt:Audience"],
				principal.Claims,
				expires: DateTime.Now.AddMinutes(120),
				signingCredentials: _credentials);
		}

		private string ValidateToken(string jwtToken)
		{
			TokenValidationParameters validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = _cs["Jwt:Issuer"],
				IssuerSigningKey = _securityKey
			};
			try
			{
				var p = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);
				return p.Identity.Name;
			}
			catch (Exception ex)
			{
				throw new AuthenticationException(ex.Message, ex);
			}
		}
	}

	public class JwtLoginProvider2
	{
		private readonly HttpContext _context;

		public JwtLoginProvider2(HttpContext context)
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
}
