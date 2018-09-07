using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public static class CryptoExtensionscs
	{
		public static byte[] ExportBin(this RSA rsa)
		{
			RSAParameters ps = rsa.ExportParameters(false);
			var modulus = ps.Modulus;
			var exponent = ps.Exponent;
			using (var stream = new MemoryStream(4 + modulus.Length + exponent.Length))
			{
				stream.Write(BitConverter.GetBytes(modulus.Length));
				stream.Write(modulus);
				stream.Write(exponent);
				return stream.ToArray();
			}
		}
		public static string Export(this RSA rsa) => Convert.ToBase64String(rsa.ExportBin());

		public static RSA ImportBin(this RSA rsa, byte[] publicKey)
		{
			using (MemoryStream stream = new MemoryStream(publicKey))
			using (BinaryReader br = new BinaryReader(stream))
			{
				int pos = br.ReadInt32();
				byte[] modulus = br.ReadBytes(pos);
				byte[] exponent = br.ReadBytes(Convert.ToInt32(stream.Length - stream.Position));
				RSAParameters ps = new RSAParameters() { Modulus = modulus, Exponent = exponent };
				rsa.ImportParameters(ps);
			}
			return rsa;
		}

		public static RSA Import(this RSA rsa, string publicKeyBase64String) =>
			rsa.ImportBin(Convert.FromBase64String(publicKeyBase64String));

		public static byte[] ExportBin(this Aes aes)
		{
			var key = aes.Key;
			var iv = aes.IV;
			using (var stream = new MemoryStream(4 + key.Length + iv.Length))
			{
				stream.Write(BitConverter.GetBytes(key.Length));
				stream.Write(key);
				stream.Write(iv);
				return stream.ToArray();
			}
		}
		public static string Export(this Aes aes) => Convert.ToBase64String(aes.ExportBin());

		public static Aes ImportBin(this Aes aes, byte[] publicKey)
		{
			using (MemoryStream stream = new MemoryStream(publicKey))
			using (BinaryReader br = new BinaryReader(stream))
			{
				int pos = br.ReadInt32();
				aes.Key = br.ReadBytes(pos);
				aes.IV = br.ReadBytes(Convert.ToInt32(stream.Length - stream.Position));
			}
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
