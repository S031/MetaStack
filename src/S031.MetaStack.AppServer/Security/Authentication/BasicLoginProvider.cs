using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S031.MetaStack.Common;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;

namespace S031.MetaStack.AppServer.Security
{
	public class BasicLoginProvider : ILoginProvider
	{
		const int _checkTicketTimeout = 5000;
		const int _expirePeriod = 3000;
		const int _expirePeriod4Loged = 7200000;
		const int _maxUserSessions = 128;

		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;
		static DateTime _lastCheckLogedTime = DateTime.Now;

		static int _tickProcess = 0;
		static readonly Timer _timer = new Timer(Tick, null, _expirePeriod, _expirePeriod);
		static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
		static readonly MapTable<string, MapTable<Guid, LoginInfo>> _users = new MapTable<string, MapTable<Guid, LoginInfo>>();

		public BasicLoginProvider(IServiceProvider services)
		{
		}

		~BasicLoginProvider()
			=> _timer.Dispose();

		/// <summary>
		/// Set timeout in milliseconds for expire client message timestamp (0-disable check)
		/// </summary>
		public int CheckTicketTimeout { get; set; } = _checkTicketTimeout;		

		/// <summary>
		/// Login request
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="clientPublicKey">client session RSA public key</param>
		/// <returns></returns>
		public async Task<string> LoginRequestAsync(string userName, string clientPublicKey)
		{
			return await Task.Run(
				()=> LoginRequest(userName, clientPublicKey)).ConfigureAwait(false);
		}

		public string LoginRequest(string userName, string clientPublicKey)
		{
			var loginInfo = new LoginInfo();
			if (_users.TryGetValue(userName, out var sessions))
				if (sessions.Count <= _maxUserSessions)
					sessions.Add(loginInfo.SessionID, loginInfo);
				else
					throw new AuthenticationException($"Invaliod session count for user {userName}, must be less then {_maxUserSessions}");
			else
				_users.Add(userName, new MapTable<Guid, LoginInfo>() { { loginInfo.SessionID, loginInfo } });

			return RSA.Create()
				.Import(clientPublicKey)
				.Encrypt(loginInfo.Export(), _padding)
				.ToBASE64String();
		}

		/// <summary>
		/// <see cref="Logon(string, string, string)"/>
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="encryptedKey">Base64 string, must be a session ticket with solt or password</param>
		/// <returns></returns>
		public async Task<string> LogonAsync(string userName, string sessionID, string encryptedKey)
		{
			return await Task.Run(
				() => Logon(userName, sessionID, encryptedKey)).ConfigureAwait(false);
		}

		public string Logon(string userName, string sessionID, string encryptedKey)
		{
			Guid sessionUID = new Guid(sessionID);
			if (!_users.ContainsKey(userName) ||
				!_users[userName].TryGetValue(sessionUID, out LoginInfo loginInfo))
				throw new AuthenticationException($"Timeout logon for user {userName} expired, or not LoginRequest called before");

			if (loginInfo.IsLogedOn())
			{
				if (!CheckTicket(loginInfo, encryptedKey.ToByteArray(), this.CheckTicketTimeout))
					throw new AuthenticationException($"Invaliod session ticked for user {userName}");
			}
			else if (CheckPassword(userName, loginInfo, encryptedKey.ToByteArray()))
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
			else if (CheckTicket(loginInfo, encryptedKey.ToByteArray(), this.CheckTicketTimeout))
				RemoveSession(userName, sessionUID);
			else
				throw new AuthenticationException($"Invaliod session ticked");

		}

		private static bool CheckTicket(LoginInfo loginInfo, byte[] encryptedTicketData, int checkTicketTimeout)
		{
			try
			{
				//В Тикет добавили проверяемое значение (таимстамп)
				byte[] data = Aes.Create()
							.ImportBin(loginInfo.CryptoKey)
							.DecryptBin(encryptedTicketData);
				Guid ticket = new Guid(data.Take(16).ToArray());
				if (checkTicketTimeout > 0)
				{
					double ms = BitConverter.ToDouble(data, 16);
					if (Math.Abs((DateTime.Now - DateTime.Now.Date).TotalMilliseconds - ms) > checkTicketTimeout)
						throw new AuthenticationException("Bad message timestamp");
				}
				return loginInfo.Ticket.Equals(ticket);
			}
			catch (Exception ex)
			{
				throw new AuthenticationException($"Incorrect session ticked data (inner exception:{ex.Message})");
			}
		}

		private static bool CheckPassword(string userName, LoginInfo loginInfo, byte[] encryptedTicketData)
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
				throw new AuthenticationException($"Incorrect password for user {userName} ({ex.Message})");
			}
		}

		private static void RemoveSession(string userName, Guid sessionID)
		{
			if (_users.ContainsKey(userName) && _users[userName].ContainsKey(sessionID))
				_users[userName].Remove(sessionID);
			if (_users[userName].Count == 0)
				_users.Remove(userName);
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
				if ((t - _lastCheckLogedTime).TotalMilliseconds > _expirePeriod4Loged)
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

		async Task<UserInfo> ILoginProvider.LogonAsync(string userName, string sessionID, string encryptedKey)
		{
			_ = await this.LogonAsync(userName, sessionID, encryptedKey);
			return null;
		}

		UserInfo ILoginProvider.Logon(string userName, string sessionID, string encryptedKey)
		{
			_ = this.Logon(userName, sessionID, encryptedKey);
			return null;
		}
	}
}
