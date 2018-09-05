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
		}
		public Guid SessionID { get; }
		public DateTime LastTime { get; }
		public string PublicKeyData { get; }
	}
}
