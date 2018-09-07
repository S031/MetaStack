using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Security;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using S031.MetaStack.Core.Logging;

namespace MetaStack.Test.Security
{
	public class CryptographyTest
	{
		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;
		public CryptographyTest()
		{
			MetaStack.Test.Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void RSATest()
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
		void AesTest()
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
		void ImpersonateTest()
		{
			Impersonator.Execute("Test", "test", () => Assert.False(false));
		}

		[Fact]
		void LogonTest()
		{
			using (FileLogger l = new FileLogger("CryptographyTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var userName = "Test";
				var secret = "@TestPassword";
				var svcProv = ApplicationContext.GetServices();
				//var serverRSA = svcProv.GetService<RSA>();
				//var serverPK = serverRSA.ExportParameters(false).Export();

				var clientRSA = RSA.Create();
				var clientPK = clientRSA.Export();
				var loginFactory = svcProv.GetService<ILoginFactory>();
				//var serverPK = loginFactory.LoginRequest(userName, clientPK);

				//var serverRSA = RSA.Create();
				//serverRSA.Import(serverPK);
				//string token = loginFactory.Logon(userName,
				//	Convert.ToBase64String(serverRSA.Encrypt(Encoding.UTF8.GetBytes(secret), _padding)));
				//Guid sessionID = new Guid(clientRSA.Decrypt(token.ToByteArray(), _padding));
				//Assert.NotEqual(sessionID, Guid.Empty);

				//l.Debug("Start performance test for 1000 logins");
				//var data = Aes.Create().ExportBin();
				//for (int i = 1; i < 100000; i++)
				//{
				//	Aes.Create().ImportBin(data).EncryptBin(sessionID.ToByteArray());
				//	token = loginFactory.Logon(userName, sessionID.ToString());
				//}
				////token = loginFactory.Logon(userName,
				////	Convert.ToBase64String(serverRSA.Encrypt(sessionID.ToByteArray(), _padding)));				
				l.Debug("End performance test for 1000 logins");
			}
		}

		RSA CreateFromPK(string publicKeyBase64String)
		{
			var rsa = RSA.Create();
			rsa.Import(publicKeyBase64String);
			return rsa;
		}
	}
}
