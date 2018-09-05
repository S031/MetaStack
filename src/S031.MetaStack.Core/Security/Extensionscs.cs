using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public static class CryptoExtensionscs
	{
		const string _separator = "---";
		public static string Export(this RSAParameters ps)
		{
			var modulus = ps.Modulus;
			var exponent = ps.Exponent;
			return $"{Convert.ToBase64String(modulus)}{_separator}{Convert.ToBase64String(exponent)}";
		}

		public static RSAParameters Import(this RSAParameters ps, string publicKeyBase64String)
		{
			int pos = publicKeyBase64String.IndexOf(_separator);
			ps.Modulus = Convert.FromBase64String(publicKeyBase64String.Left(pos));
			ps.Exponent = Convert.FromBase64String(publicKeyBase64String.Substring(pos + _separator.Length));
			return ps;
		}
	}
}
