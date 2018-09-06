using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Security;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MetaStack.Test.Security
{
	public class CryptographyTest
	{
		public CryptographyTest()
		{
			MetaStack.Test.Program.ConfigureTests();
		}
		[Fact]
		void RSATest()
		{
			//var data = Encoding.UTF8.GetBytes("Data To Be Encrypted");
			var data = "Данные для шифрования";

			var rsaServer = RSA.Create();
			var toSendPK = rsaServer.ExportParameters(false).Export();

			var encryptedData = Encrypt(toSendPK, data);

			var decryptedData = rsaServer.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.OaepSHA256);
			Assert.Equal(data, Encoding.UTF8.GetString(decryptedData));
		}

		private string Encrypt(string pPublicKey, string pInputString)
		{
			//Create a new instance of the RSACryptoServiceProvider class.
			var lRSA = RSA.Create();

			//Import key parameters into RSA.
			lRSA.ImportParameters(new RSAParameters().Import(pPublicKey));

			return Convert.ToBase64String(lRSA.Encrypt(Encoding.UTF8.GetBytes(pInputString), RSAEncryptionPadding.OaepSHA256));
		}
		[Fact]
		void ImpersonateTest()
		{
			Impersonator.Execute("Test", "test", () => Assert.False(false));
		}

		[Fact]
		void LogonTest()
		{

			var loginFactory = ApplicationContext.GetServices().GetService<ILoginFactory>().LoginRequest(userName, publicKey);

		}

	}
}
