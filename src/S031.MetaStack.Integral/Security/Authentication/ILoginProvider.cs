using S031.MetaStack.Integral.Security;
using System.Threading.Tasks;

namespace S031.MetaStack.Security
{
	public interface ILoginProvider
	{
		Task<string> LoginRequestAsync(string userName, string clientPublicKey);
		string LoginRequest(string userName, string clientPublicKey);
		Task<UserInfo> LogonAsync(string userName, string sessionID, string encryptedKey);
		UserInfo Logon(string userName, string sessionID, string encryptedKey);
		Task LogoutAsync(string userName, string sessionID, string encryptedKey);
		void Logout(string userName, string sessionID, string encryptedKey);
	}
}
