using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public class LoginInfo
	{
		public LoginInfo(string publicKeyData)
		{
			SessionID = Guid.NewGuid();
			LastTime = DateTime.Now;
			PublicKeyData = publicKeyData;
		}
		public Guid SessionID { get; }
		public DateTime LastTime { get; set; }
		public string PublicKeyData { get; }
		public byte[] RsaModulus { get; }
		public byte[] RsaExponent { get; }
		public byte[] AesKey { get; set; }
		public byte[] AesIV { get; set; }
	}
}
