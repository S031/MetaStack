using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.Security
{
	public class BasicLoginFactory : ILoginFactory
	{
		const int expirePeriod = 3000;
		const int expirePeriod4Loged = 7200000;
		const int maxUserSessions = 64;

		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;
		static readonly object obj4Lock = new object();
		static readonly object obj4LockUser = new object();
		static DateTime _lastCheckLogedTime = DateTime.Now;
		static readonly Timer _timer = new Timer(Tick, null, expirePeriod, expirePeriod);

		static readonly Dictionary<string, LoginInfo> _notLogedUsers = new Dictionary<string, LoginInfo>();
		static readonly Dictionary<string, Dictionary<Guid, LoginInfo>> _logedUsers = new Dictionary<string, Dictionary<Guid, LoginInfo>>();

		public BasicLoginFactory()
		{
		}
		
		/// <summary>
		/// Login request
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="clientPublicKey">client session RSA public key</param>
		/// <returns></returns>
		public string LoginRequest(string userName, string clientPublicKey)
		{
			bool userExists = _notLogedUsers.ContainsKey(userName);
			if (userExists)
			{
				//wait for another login with same name
				for (int i = 0; (i < expirePeriod || userExists); i += 100)
				{
					Thread.Sleep(100);
					userExists = _notLogedUsers.ContainsKey(userName);
				}
				if (userExists)
					throw new AuthenticationExceptions("Timeout expired");
			}
			var loginInfo = new LoginInfo();
			lock (obj4Lock)
				_notLogedUsers.Add(userName, loginInfo);

			return RSA.Create()
				.Import(clientPublicKey)
				.Encrypt(loginInfo.Export(), _padding)
				.ToBASE64String();
		}
		
		/// <summary>
		/// !!! Add solt
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Base64 string, must be a sessionID and passwors</param>
		/// <returns></returns>
		public string Logon(string userName, string sessionID, string encryptedKey)
		{
			byte[] data;
			var requestExists = _notLogedUsers.TryGetValue(userName, out LoginInfo loginInfo);

			if (requestExists)
			{

			}
			else if (!_logedUsers.ContainsKey(userName))
				throw new AuthenticationExceptions($"Timeout logon for user {userName} expired, or not LoginRequest called before");
			else
			{ }

			//if (!Guid.TryParse(encryptedKey, out Guid sessionID))
			//	sessionID = Guid.Empty;

			////var sessionID = GetSessionIDFromDecryptedData(data);
			//;
			//if (sessionID != Guid.Empty && userExists && _logedUsers[userName].ContainsKey(sessionID))
			//{
			//	lock (obj4Lock)
			//		_logedUsers[userName][sessionID].LastTime = DateTime.Now;
			//	//return Encrypt(_logedUsers[userName][sessionID].PublicKeyData, sessionID.ToByteArray());
			//	return sessionID.ToString();
			//}
			//else if (sessionID != Guid.Empty)
			//	throw new AuthenticationExceptions($"Session token used for login {userName} not found");

			//	if (data == null)
			//		throw new AuthenticationExceptions($"Can't decrypt data for login user {userName}");

			//	string password = Encoding.UTF8.GetString(data);
			//	Impersonator.Execute(userName, password, () => { return; });
			//	var newInfo = new LoginInfo(loginInfo.PublicKeyData);
			//	sessionID = newInfo.SessionID;
			//	lock (obj4Lock)
			//	{
			//		if (!_logedUsers.ContainsKey(userName))
			//			_logedUsers.Add(userName, new Dictionary<Guid, LoginInfo>() { { sessionID, newInfo } });
			//		else if (_logedUsers[userName].Count >= maxUserSessions)
			//			throw new AuthenticationExceptions($"Active sessions count for user {userExists} less then {maxUserSessions}");
			//		else
			//			_logedUsers[userName].Add(sessionID, newInfo);
			//	}
			//	return Encrypt(newInfo.PublicKeyData, newInfo.SessionID.ToByteArray());
			//}
			return "";
		}

		private Guid GetSessionIDFromDecryptedData(byte[] data)
		{
			try
			{
				return new Guid(data);
			}
			catch
			{
				return Guid.Empty;
			}
		}

		/// <summary>
		/// End session
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Sesion token</param>
		public void Logout(string userName, string encryptedKey)
		{
			throw new NotImplementedException();
		}

		private static void Tick(object state)
		{
			var t = DateTime.Now;
			List<(string, string)> expiredInfo = new List<(string, string)>();
			expiredInfo.AddRange(
				_notLogedUsers
				.Where(kvp => (t - kvp.Value.LastTime).TotalMilliseconds > expirePeriod)
				.Select(kvp => (kvp.Key, kvp.Key)));

			foreach (var data in expiredInfo)
				if (_notLogedUsers.ContainsKey(data.Item1))
					lock (obj4Lock)
						_notLogedUsers.Remove(data.Item1);

			if ((t - _lastCheckLogedTime).Milliseconds > expirePeriod4Loged)
			{
				expiredInfo = new List<(string, string)>();
				foreach (var uInfo in _logedUsers)
					expiredInfo.AddRange(
						uInfo.Value
							.Where(kvp => (t - kvp.Value.LastTime).TotalMilliseconds > expirePeriod4Loged)
							.Select(kvp => (uInfo.Key, kvp.Key.ToString())));

				foreach (var data in expiredInfo)
					if (_logedUsers.ContainsKey(data.Item1))
						lock (obj4Lock)
						{
							_logedUsers[data.Item1].Remove(new Guid(data.Item2));
							if (_logedUsers[data.Item1].Count == 0)
								_logedUsers.Remove(data.Item1);
						}
				_lastCheckLogedTime = t;
			}
		}
	}
}
