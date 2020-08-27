using S031.MetaStack.Common.Logging;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
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
				string s = u.ToString(S031.MetaStack.Json.Formatting.Indented);
				_logger.Debug(s);
				u = UserInfo.ReadFrom(s);
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
					u = UserInfo.ReadFrom(s);

				s = u.ToString(S031.MetaStack.Json.Formatting.Indented);
				_logger.Debug(s);
			}
		}

		private static UserInfo GetCustomPrincipal()
		{
			string userName = Environment.UserName;
			string name = "Joe Smith";

			ClaimsIdentity objClaim = new ClaimsIdentity("Basic", ClaimTypes.Dns, ClaimTypes.Role);
			objClaim.AddClaim(new Claim(ClaimTypes.Dns, userName));
			objClaim.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, "Basic"));
			objClaim.AddClaim(new Claim(ClaimTypes.Name, name));
			objClaim.AddClaim(new Claim(ClaimTypes.Email, "test@server.com"));

			UserInfo MyPrincipal = new UserInfo(objClaim);
			MyPrincipal.AccessLevelID = 1;
			MyPrincipal.DomainName = "test";
			MyPrincipal.Name = name;
			MyPrincipal.PersonID = 0;
			MyPrincipal.StructuralUnitID = 1;
			return MyPrincipal;
		}
	}
}
