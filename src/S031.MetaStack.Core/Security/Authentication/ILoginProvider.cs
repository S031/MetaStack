using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Security
{
	public interface ILoginProvider
	{
		Task<string> LoginRequestAsync(string userName, string clientPublicKey);
		string LoginRequest(string userName, string clientPublicKey);
		Task<string> LogonAsync(string userName, string sessionID, string encryptedKey);
		string Logon(string userName, string sessionID, string encryptedKey);
		Task LogoutAsync(string userName, string sessionID, string encryptedKey);
		void Logout(string userName, string sessionID, string encryptedKey);
	}
}
