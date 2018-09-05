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
		static readonly object obj4Lock = new object();
		static DateTime _lastCheckLogedTime = DateTime.Now;
		static readonly Timer _timer = new Timer(Tick, null, expirePeriod, expirePeriod);
		static readonly Dictionary<string, LoginInfo> _notLogedUsers = new Dictionary<string, LoginInfo>();
		static readonly Dictionary<string, LoginInfo> _logedUsers = new Dictionary<string, LoginInfo>();
		private RSA _rsa;

		public BasicLoginFactory()
		{
			_rsa = App.ApplicationContext.GetServices().GetService<RSA>();
		}
		public string LoginRequest(string userName, string clientPublicKey)
		{
			lock(obj4Lock)
			{
				if (_notLogedUsers.ContainsKey(userName))
					_notLogedUsers.Remove(userName);
				if (_logedUsers.ContainsKey(userName))
					_logedUsers.Remove(userName);
				_notLogedUsers.Add(userName, new LoginInfo(clientPublicKey));
			}
			return _rsa.ExportParameters(false).Export();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Base64 string</param>
		/// <returns></returns>
		public string Logon(string userName, string encryptedKey)
		{
			if (!_notLogedUsers.TryGetValue(userName, out LoginInfo loginInfo))
				throw new InvalidOperationException($"Timeout logon for user {userName} expired, or not LoginRequest called before");
			string password = Encoding.UTF8.GetString(_rsa.Decrypt(encryptedKey.ToByteArray(), null));
			Impersonator.Execute(userName, password, ()=> { return; } );
			var newInfo = new LoginInfo(loginInfo.PublicKeyData);
			var rsa = RSA.Create();
			rsa.ImportParameters(new RSAParameters().Import(newInfo.PublicKeyData));
			return rsa.Decrypt(newInfo.SessionID.ToByteArray(), null).ToBASE64String();
		}

		public void Logout(string userName, string encryptedKey)
		{
			throw new NotImplementedException();
		}
		private static void Tick(object state)
		{
			var t = DateTime.Now;
			List<string> expiredInfo = _notLogedUsers
				.Where(kvp => (t - kvp.Value.LastTime).TotalMilliseconds > expirePeriod)
				.Select(kvp => kvp.Key).ToList();
			foreach (var user in expiredInfo)
				lock (obj4Lock)
					_notLogedUsers.Remove(user);
			if ((t-_lastCheckLogedTime).Milliseconds > expirePeriod4Loged)
			{
				expiredInfo = _logedUsers
					.Where(kvp => (t - kvp.Value.LastTime).TotalMilliseconds > expirePeriod4Loged)
					.Select(kvp => kvp.Key).ToList();
				foreach (var user in expiredInfo)
					lock (obj4Lock)
						_logedUsers.Remove(user);

			}
		}
	}
}
