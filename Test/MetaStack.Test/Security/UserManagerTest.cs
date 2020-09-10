using S031.MetaStack.Common.Logging;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Xunit;

namespace MetaStack.Test.Security
{
	public class UserManagerTest
	{
		public UserManagerTest()
		{
			MetaStack.Test.Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		private void GetUserInfoTest()
		{
			var u = GetCustomPrincipal();
			using (FileLog _logger = new FileLog("MetaStackSecurity.GetUserInfoTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				string s = u.ToString(S031.MetaStack.Json.Formatting.None);
				_logger.Debug(s);
				u = UserInfo.Parse(s);
				s = u.ToString(S031.MetaStack.Json.Formatting.Indented);
				_logger.Debug(s);
			}
		}

		[Fact]
		private void UserInfoSerializationTest()
		{
			var u = GetCustomPrincipal();
			using (FileLog _logger = new FileLog("MetaStackSecurity.UserInfoSerializationTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				string s = JsonSerializer.SerializeObject(u);
				_logger.Debug(s);
				u = (UserInfo)JsonSerializer.DeserializeObject(typeof(UserInfo), s);
				s = JsonSerializer.SerializeObject(u);
				_logger.Debug(s);
			}
		}

		[Fact]
		private void UserInfoSerializationSpeedTest()
		{
			var u = GetCustomPrincipal();
			using (FileLog _logger = new FileLog("MetaStackSecurity.UserInfoSerializationSpeedTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				_logger.Debug("Start UserInfoSerializationSpeedTest");
				string s = "";
				for (int i = 0; i < 100_000; i++)
					s = u.ToString(S031.MetaStack.Json.Formatting.Indented);

				_logger.Debug(s);
				_logger.Debug("Start UserInfoDeSerializationSpeedTest");
				for (int i = 0; i < 100_000; i++)
					u = UserInfo.Parse(s);

				s = u.ToString(S031.MetaStack.Json.Formatting.Indented);
				_logger.Debug(s);
			}
		}

		private static UserInfo GetCustomPrincipal()
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

			UserInfo currentPrincipal = new UserInfo(objClaim);
			currentPrincipal.AccessLevelID = 1;
			currentPrincipal.DomainName = domainName;
			currentPrincipal.Name = name;
			currentPrincipal.PersonID = 0;
			currentPrincipal.StructuralUnitID = 1;
			currentPrincipal.Roles.Add("Everyone");
			return currentPrincipal;
		}
	}
}
