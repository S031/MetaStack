using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Security;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace MetaStack.Test.Security
{
	public class CryptographyTest
	{
		private static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;
		public CryptographyTest()
		{
			MetaStack.Test.Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		private void RSATest()
		{
			//var data = Encoding.UTF8.GetBytes("Data To Be Encrypted");
			var data = "Данные для шифрования";

			var rsaServer = RSA.Create();
			var toSendPK = rsaServer.Export();

			var encryptedData = Encrypt(toSendPK, Encoding.UTF8.GetBytes(data));

			var decryptedData = rsaServer.Decrypt(Convert.FromBase64String(encryptedData), _padding);
			Assert.Equal(data, Encoding.UTF8.GetString(decryptedData));
		}
		[Fact]
		private void AesTest()
		{
			Guid data = Guid.NewGuid();
			var aesServer = Aes.Create();
			var key = aesServer.ExportBin();

			var encryptedData = aesServer.EncryptBin(data.ToByteArray());
			Assert.Equal(data, new Guid(Aes.Create().ImportBin(key).DecryptBin(encryptedData)));
		}

		private string Encrypt(string pPublicKey, byte[] pInput)
		{
			return Convert.ToBase64String(
				RSA.Create()
				.Import(pPublicKey)
				.Encrypt(pInput, _padding));
		}

		[Fact]
		private void ImpersonateTest()
		{
			Impersonator.Execute<bool>("Test", "@TestPassword", () => true);
		}

		[Fact]
		private void LogonTest()
		{
			using (FileLogger l = new FileLogger("CryptographyTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var userName = "Test";
				var secret = "@TestPassword";
				var svcProv = ApplicationContext.GetServices();

				var clientRSA = RSA.Create();
				var clientPK = clientRSA.Export();
				var loginFactory = svcProv.GetService<ILoginProvider>();
				var secretData = loginFactory.LoginRequest(userName, clientPK)
					.ToByteArray();

				LoginInfo loginInfo = new LoginInfo();
				loginInfo.Import(clientRSA.Decrypt(secretData, _padding));

				var clientAes = Aes.Create()
					.ImportBin(loginInfo.CryptoKey);
				string token = loginFactory.Logon(userName, loginInfo.SessionID.ToString(),
					clientAes
					.EncryptBin(Encoding.UTF8.GetBytes(secret))
					.ToBASE64String());

				Guid ticket = new Guid(clientAes.DecryptBin(token.ToByteArray()).Take(16).ToArray());
				Assert.NotEqual(ticket, Guid.Empty);

				l.Debug("Start performance test for logins");
				int i;
				for (i = 0; i < 10000; i++)
				{
					token = loginFactory.Logon(userName, loginInfo.SessionID.ToString(),
						clientAes.EncryptBin(ticket
							.ToByteArray()
							.Concat(BitConverter.GetBytes(DateTime.Now.Millisecond)).ToArray())
							.ToBASE64String());
				}
				l.Debug($"End performance test for {i} logins");
			}
		}

		private RSA CreateFromPK(string publicKeyBase64String)
		{
			var rsa = RSA.Create();
			rsa.Import(publicKeyBase64String);
			return rsa;
		}
	}
}
