using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Caching;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskPlus.Server.Api.Properties;
using TaskPlus.Server.Data;

namespace TaskPlus.Server.Security
{
	public class UserManager : UserManagerBase
	{
		private readonly IServiceProvider _services;
		//private readonly IConfiguration _config;
		//private readonly ILogger _logger;
		//private readonly IAuthorizationProvider _authorizationProvider;

		private static readonly UserInfoCache _uiCache = UserInfoCache.Instance;

		public UserManager(IServiceProvider services)
		{
			_services = services;
			//_config = _services.GetRequiredService<IConfiguration>();
			//_logger = services.GetRequiredService<ILogger>();
			//_authorizationProvider = services.GetRequiredService<IAuthorizationProvider>();
			mdb = _services
				.GetRequiredService<IMdbContextFactory>()
				.GetContext(Strings.SysCatConnection);
		}
		public async override Task<UserInfo> GetUserInfoAsync(string login)
		{
			if (!_uiCache.TryGetValue(login, out UserInfo ui))
			{
				ui = await base.GetUserInfoAsync(login);
				//if user not exists added null value
				_uiCache.TryAdd(login, ui);
			}
			return ui;
		}
		public static UserInfo GetCurrentPrincipal()
		{
			string userName = Environment.UserName;
			string domainName = Environment.UserDomainName.ToLower();
			string name = $@"{domainName}\{userName}";
			string region = new RegionInfo(CultureInfo.CurrentCulture.LCID).TwoLetterISORegionName.ToLower();
			string zone = region == "en" ? "com" : region;

			ClaimsIdentity objClaim = new ClaimsIdentity("Basic", ClaimTypes.Dns, ClaimTypes.Role);
			objClaim.AddClaim(new Claim(ClaimTypes.Dns, userName));
			objClaim.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, "Basic"));
			objClaim.AddClaim(new Claim(ClaimTypes.Name, name));
			objClaim.AddClaim(new Claim(ClaimTypes.Email, $"{userName}@{domainName}.{zone}"));

			UserInfo currentPrincipal = new UserInfo(objClaim)
			{
				AccessLevelID = 1,
				DomainName = domainName,
				Name = name,
				PersonID = 0,
				StructuralUnitID = 1
			};
			currentPrincipal.Roles.Add("Everyone");
			return currentPrincipal;
		}
	}
}
