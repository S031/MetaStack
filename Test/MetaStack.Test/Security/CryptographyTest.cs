using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
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
			MetaStack.Test.Program.GetServices();
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
			using (FileLog l = new FileLog("CryptographyTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var userName = "svostrikov@metib.ru";
				var secret = "@test";
				var svcProv = Program.GetServices();

				var clientRSA = RSA.Create();
				var clientPK = clientRSA.Export();
				var loginProvider = svcProv.GetService<ILoginProvider>();
				var secretData = loginProvider.LoginRequest(userName, clientPK)
					.ToByteArray();

				LoginInfo loginInfo = new LoginInfo();
				loginInfo.Import(clientRSA.Decrypt(secretData, _padding));

				var clientAes = Aes.Create()
					.ImportBin(loginInfo.CryptoKey);
				string token = loginProvider.Logon(userName, loginInfo.SessionID.ToString(),
					clientAes
					.EncryptBin(Encoding.UTF8.GetBytes(secret))
					.ToBASE64String()).SessionToken;

				Guid ticket = new Guid(clientAes.DecryptBin(token.ToByteArray()).Take(16).ToArray());
				Assert.NotEqual(ticket, Guid.Empty);

				l.Debug("Start performance test for logins");
				int i;
				for (i = 0; i < 10000; i++)
				{
					token = loginProvider.Logon(userName, loginInfo.SessionID.ToString(),
						clientAes.EncryptBin(ticket
							.ToByteArray()
							.Concat(BitConverter.GetBytes((DateTime.Now - DateTime.Now.Date).TotalMilliseconds)).ToArray())
							.ToBASE64String()).SessionToken;
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

		[Fact]
		private void HashAlgorithmTest()
		{
			const string password_str = "@test";
			using (FileLog l = new FileLog("CryptographyTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug($"Password (@test) sha256 hash: {CryptoHelper.ComputeSha256Hash(password_str)}");
				l.Debug($"Password (@test) md5 hash: {CryptoHelper.ComputeMD5Hash(password_str)}");
			}
		}
	}
}
