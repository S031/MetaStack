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
		private readonly IUserManager _userManager;
		//private readonly ILogger _logger;

		private readonly IConfigurationSection _cs;
		private readonly SymmetricSecurityKey _securityKey;
		private readonly SigningCredentials _credentials;


		public JwtLoginProvider(IServiceProvider services)
		{
			_services = services;
			_config = _services.GetRequiredService<IConfiguration>();
			//_logger = _services.GetRequiredService<ILogger>();
			_userManager = _services.GetRequiredService<IUserManager>();

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

		/// <summary>
		////Validate jwt token and retrieve UserInfo
		/// </summary>
		/// <param name="userName">Not used</param>
		/// <param name="sessionID">Not Used</param>
		/// <param name="encryptedKey">Base64 encoded jwt token</param>
		/// <returns><see cref="UserInfo"/></returns>
		public UserInfo Logon(string userName, string sessionID, string encryptedKey)
			=> LogonAsync(userName, sessionID, encryptedKey)
			.GetAwaiter()
			.GetResult();

		/// <summary>
		////Validate jwt token and retrieve UserInfo
		/// </summary>
		/// <param name="userName">Not used</param>
		/// <param name="sessionID">Not Used</param>
		/// <param name="encryptedKey">Base64 encoded jwt token</param>
		/// <returns><see cref="UserInfo"/></returns>
		public async Task<UserInfo> LogonAsync(string userName, string sessionID, string encryptedKey)
		{
			var login = ValidateToken(encryptedKey);
			return await _userManager.GetUserInfoAsync(login);
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
			var ui = await _userManager.GetUserInfoAsync(userName);
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
				ValidateAudience = false,
				ValidateIssuerSigningKey = true,
				ValidIssuer = _cs["Jwt:Issuer"],
				IssuerSigningKey = _securityKey
			};
			try
			{
				var p = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);
				return p.Claims.FirstOrDefault(claim=>claim.Type == ClaimTypes.Email)?.Value ;
			}
			catch (Exception ex)
			{
				throw new AuthenticationException(ex.Message, ex);
			}
		}
	}
}
