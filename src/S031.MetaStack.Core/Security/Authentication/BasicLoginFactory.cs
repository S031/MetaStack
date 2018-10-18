using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.Security
{
	public class BasicLoginFactory : ILoginFactory
	{
		const int _expirePeriod = 3000;
		const int _expirePeriod4Loged = 7200000;
		const int _maxUserSessions = 64;

		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;
		static readonly object obj4Lock = new object();
		static readonly object obj4LockUser = new object();
		static DateTime _lastCheckLogedTime = DateTime.Now;

		static int _tickProcess = 0;
		static readonly Timer _timer = new Timer(Tick, null, _expirePeriod, _expirePeriod);
		static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
		static readonly Dictionary<string, Dictionary<Guid, LoginInfo>> _users = new Dictionary<string, Dictionary<Guid, LoginInfo>>();

		public BasicLoginFactory()
		{
		}
		
		/// <summary>
		/// Login request
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="clientPublicKey">client session RSA public key</param>
		/// <returns></returns>
		public async Task<string> LoginRequestAsync(string userName, string clientPublicKey)
		{
			return await Task.Factory.StartNew(
				()=> LoginRequest(userName, clientPublicKey)).ConfigureAwait(false);
		}

		public string LoginRequest(string userName, string clientPublicKey)
		{
			var loginInfo = new LoginInfo();
			if (_users.ContainsKey(userName))
				lock (obj4Lock)
					_users[userName].Add(loginInfo.SessionID, loginInfo);
			else
				lock (obj4Lock)
					_users.Add(userName, new Dictionary<Guid, LoginInfo>() { { loginInfo.SessionID, loginInfo } });

			return RSA.Create()
				.Import(clientPublicKey)
				.Encrypt(loginInfo.Export(), _padding)
				.ToBASE64String();
		}

		/// <summary>
		/// !!! Add solt
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Base64 string, must be a session ticket with solt or password</param>
		/// <returns></returns>
		public async Task<string> LogonAsync(string userName, string sessionID, string encryptedKey)
		{
			return await Task.Factory.StartNew(
				() => Logon(userName, sessionID, encryptedKey)).ConfigureAwait(false);
		}

		public string Logon(string userName, string sessionID, string encryptedKey)
		{
			Guid sessionUID = new Guid(sessionID);
			if (!_users.ContainsKey(userName) ||
				!_users[userName].TryGetValue(sessionUID, out LoginInfo loginInfo))
				throw new AuthenticationExceptions($"Timeout logon for user {userName} expired, or not LoginRequest called before");

			if (loginInfo.IsLogedOn())
			{
				if (!CheckTicket(userName, loginInfo, encryptedKey.ToByteArray()))
					throw new AuthenticationExceptions($"Invaliod session ticked for user {userName}");
			}
			else if (CheckPasswordAsync(userName, loginInfo, encryptedKey.ToByteArray()))
				loginInfo.EmmitTicket();

			return Aes.Create()
				.ImportBin(loginInfo.CryptoKey)
				.EncryptBin(loginInfo.Ticket.ToByteArray()
					.Concat(BitConverter.GetBytes(_random.Next()))
					.ToArray())
				.ToBASE64String();
		}


		/// <summary>
		/// End session
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Sesion token</param>
		public async Task LogoutAsync(string userName, string sessionID, string encryptedKey)
		{
			await Task.Factory.StartNew(
				() => Logout(userName, sessionID, encryptedKey)).ConfigureAwait(false);
		}

		public void Logout(string userName, string sessionID, string encryptedKey)
		{
			Guid sessionUID = new Guid(sessionID);
			if (!_users.ContainsKey(userName) ||
				!_users[userName].TryGetValue(sessionUID, out LoginInfo loginInfo))
				return;

			if (!loginInfo.IsLogedOn())
				RemoveSession(userName, sessionUID);
			else if (CheckTicket(userName, loginInfo, encryptedKey.ToByteArray()))
				RemoveSession(userName, sessionUID);
			else
				throw new AuthenticationExceptions($"Invaliod session ticked");

		}

		private static bool CheckTicket(string userName, LoginInfo loginInfo, byte[] encryptedTicketData)
		{
			try
			{
				return loginInfo.Ticket.Equals(
						new Guid(
							Aes.Create()
							.ImportBin(loginInfo.CryptoKey)
							.DecryptBin(encryptedTicketData)
							.Take(16)
							.ToArray()
						));
			}
			catch (Exception ex)
			{
				throw new AuthenticationExceptions($"Incorrect session ticked data (inner exception:{ex.Message})");
			}
		}

		private static bool CheckPasswordAsync(string userName, LoginInfo loginInfo, byte[] encryptedTicketData)
		{

			try
			{
				string password = Encoding.UTF8.GetString(
					Aes.Create()
					.ImportBin(loginInfo.CryptoKey)
					.DecryptBin(encryptedTicketData));
				Impersonator.Execute<bool>(userName, password, () => true);
				return true;
			}
			catch (Exception ex)
			{
				throw new AuthenticationExceptions($"Incorrect password for user {userName} ({ex.Message})");
			}
		}

		private static void RemoveSession(string userName, Guid sessionID)
		{
			if (_users.ContainsKey(userName) && _users[userName].ContainsKey(sessionID))
				lock (obj4Lock)
				{
					_users[userName].Remove(sessionID);
					if (_users[userName].Count == 0)
						_users.Remove(userName);
				}
		}

		private static void Tick(object state)
		{
			if (_tickProcess > 0)
				return;

			var t = DateTime.Now;
			Interlocked.Increment(ref _tickProcess);
			try
			{
				RemoveExpired(t, false);
				if ((t - _lastCheckLogedTime).Milliseconds > _expirePeriod4Loged)
				{
					RemoveExpired(t, true);
					_lastCheckLogedTime = t;
				}
			}
			finally
			{
				Interlocked.Decrement(ref _tickProcess);
			}
		}

		private static void RemoveExpired(DateTime t, bool forLoged)
		{
			int expiredPeriod = forLoged ? _expirePeriod4Loged : _expirePeriod;
			var expiredInfo = new List<(string, Guid)>();

			foreach (var uInfo in _users)
				expiredInfo.AddRange(
					uInfo.Value
						.Where(kvp => kvp.Value.IsLogedOn() == forLoged &&
							(t - kvp.Value.LastTime).TotalMilliseconds > expiredPeriod)
						.Select(kvp => (uInfo.Key, kvp.Key)));

			foreach (var data in expiredInfo)
				RemoveSession(data.Item1, data.Item2);
		}
	}
}
