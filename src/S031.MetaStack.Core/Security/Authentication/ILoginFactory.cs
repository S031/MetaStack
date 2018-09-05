using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public interface ILoginFactory
	{
		string LoginRequest(string userName, string clientPublicKey);
		string Logon(string userName, string encryptedKey);
		void Logout(string userName, string encryptedKey);
	}
}
