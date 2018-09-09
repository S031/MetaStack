using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#if NETCOREAPP
namespace S031.MetaStack.Core.Security
#else
namespace S031.MetaStack.WinForms.Security
#endif
{
	public static class CryptoExtensions
	{
		public static byte[] ExportBin(this RSA rsa)
		{
			RSAParameters ps = rsa.ExportParameters(false);
			return BitConverter.GetBytes(ps.Modulus.Length)
				.Concat(ps.Modulus)
				.Concat(ps.Exponent)
				.ToArray();
		}
		public static string Export(this RSA rsa) => Convert.ToBase64String(rsa.ExportBin());

		public static RSA ImportBin(this RSA rsa, byte[] publicKey)
		{
			int modLen = BitConverter.ToInt32(publicKey.Take(4).ToArray(), 0);
			RSAParameters ps = new RSAParameters()
			{
				Modulus = publicKey.Skip(4).Take(modLen).ToArray(),
				Exponent = publicKey.Skip(4 + modLen).Take(publicKey.Length - 4 - modLen).ToArray()
			};
			rsa.ImportParameters(ps);
			return rsa;
		}

		public static RSA Import(this RSA rsa, string publicKeyBase64String) =>
			rsa.ImportBin(Convert.FromBase64String(publicKeyBase64String));

		public static byte[] ExportBin(this Aes aes)
		{
			return BitConverter.GetBytes(aes.Key.Length)
				.Concat(aes.Key)
				.Concat(aes.IV)
				.ToArray();
		}
		public static string Export(this Aes aes) => Convert.ToBase64String(aes.ExportBin());

		public static Aes ImportBin(this Aes aes, byte[] publicKey)
		{
			int keyLen = BitConverter.ToInt32(publicKey.Take(4).ToArray(), 0);
			aes.Key = publicKey.Skip(4).Take(keyLen).ToArray();
			aes.IV = publicKey.Skip(4 + keyLen).Take(publicKey.Length - 4 - keyLen).ToArray();
			return aes;
		}

		public static Aes Import(this Aes aes, string publicKeyBase64String) =>
			aes.ImportBin(Convert.FromBase64String(publicKeyBase64String));

		public static byte[] EncryptBin(this Aes aes, byte[] data)=>
			aes.CreateEncryptor(aes.Key, aes.IV)
			.TransformFinalBlock(data, 0, data.Length);

		public static byte[] DecryptBin(this Aes aes, byte[] data)=>
			aes.CreateDecryptor(aes.Key, aes.IV)
			.TransformFinalBlock(data, 0, data.Length);

	}

}
