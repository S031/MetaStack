using System;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using S031.MetaStack.Common;
using System.Linq;
using S031.MetaStack.Data;
using S031.MetaStack.Security;
using S031.MetaStack.Buffers;

namespace S031.MetaStack.Interop.Connectors
{
	public class TCPConnector : IDisposable
	{
		const string _endPointConfigName = "TCPConnector";
		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;

		private readonly ConnectorOptions _options;
		private Guid _ticket;
		private Aes _clientAes;
		private LoginInfo _loginInfo;

		/// <summary>
		/// Create default <see cref="TCPConnector"/> with host=localhost && port=8001
		/// </summary>
		/// <returns></returns>
		public static TCPConnector Create() => new TCPConnector(new ConnectorOptions());

		public static TCPConnector Create(ConnectorOptions options) => new TCPConnector(options);

		TCPConnector()
		{
			_options = new ConnectorOptions();
			Connected = false;
		}

		TCPConnector(ConnectorOptions options)
		{
			_options = options;
			Connected = false;
		}

		public ConnectorOptions ConnectorOptions => _options;

		public bool Connected { get; private set; } = false;

		public void Dispose()
		{
			Disconnect();
			_clientAes?.Dispose();
			SocketPool.Clear();
		}

		public TCPConnector Connect(string userName, string password)
		{
			if (!Connected)
				Connecting(userName, password);
			return this;
		}
		private void Disconnect()
		{
			Execute("Sys.Logout")
				.Dispose();
		}
		public DataPackage Execute(string actionID)
		{
			using (var paramTable = new DataPackage(new string[] { "Default.String.10" }))
				return Execute(actionID, paramTable);
		}

		public DataPackage Execute(string actionID, DataPackage paramTable)
		{
			//for paranoya mode
			//var request = new DataPackage(paramTable.ToArray());
			if (paramTable == null)
				paramTable = new DataPackage("Col1.Int.1.Null");
			paramTable.Headers["ActionID"] = actionID;
			paramTable.Headers["UserName"] = _options.UID;
			paramTable.Headers["SessionID"] = _loginInfo.SessionID.ToString();
			//Использовать таймстамп в сообщении и проверять его на сервере
			paramTable.Headers["EncryptedKey"] = _clientAes
					.EncryptBin(_ticket
					.ToByteArray()
					.Concat(BitConverter.GetBytes((DateTime.Now - DateTime.Now.Date).TotalMilliseconds)).ToArray())
					.ToBASE64String();
			paramTable.UpdateHeaders();
			var response = SendAndRecieve(paramTable);
			if (response.Headers.ContainsKey("Status") &&
				(string)response.Headers["Status"] == "ERROR")
				throw new TCPConnectorException(response);
			return response;
		}

		private void Connecting(string userName, string password)
		{
#if NETCOREAPP
			using (var clientRSA = RSA.Create())
#else
			using (var clientRSA = new RSACng())
#endif
			{
				var clientPK = clientRSA.Export();
				using (DataPackage request = new DataPackage("UserName.String.32", "PublicKey.String.256"))
				{
					request.Headers.Add("ActionID", "Sys.LoginRequest");
					request.Headers.Add("UserName", userName);
					request.UpdateHeaders();
					request.AddNew();
					request["UserName"] = userName;
					request["PublicKey"] = clientPK;
					request.Update();
					using (var response = SendAndRecieve(request))
					{
						if (response.Headers.ContainsKey("Status") &&
							(string)response.Headers["Status"] == "ERROR")
							throw new TCPConnectorException(response);

						response.GoDataTop();
						response.Read();
						_loginInfo = new LoginInfo();
						_loginInfo.Import(clientRSA.Decrypt(((string)response["LoginInfo"]).ToByteArray(), _padding));
						_clientAes = Aes.Create()
							.ImportBin(_loginInfo.CryptoKey);
					}
				}

				using (var request = new DataPackage("UserName.String.32", "SessionID.String.34", "EncryptedKey.String.256"))
				{
					request.Headers.Add("ActionID", "Sys.Logon");
					request.Headers.Add("UserName", userName);
					request.UpdateHeaders();
					request.AddNew();
					request["UserName"] = userName;
					request["SessionID"] = _loginInfo.SessionID.ToString();
					request["EncryptedKey"] = _clientAes
							.EncryptBin(Encoding.UTF8.GetBytes(password))
							.ToBASE64String();
					request.Update();
					using (var response = SendAndRecieve(request))
					{
						if (response.Headers.ContainsKey("Status") &&
							(string)response.Headers["Status"] == "ERROR")
							throw new TCPConnectorException(response);

						response.GoDataTop();
						response.Read();
						var token = (string)response["Ticket"];
						//_ticket = new Guid(_clientAes.DecryptBin(token.ToByteArray()).Take(16).ToArray());
						_ticket = new Guid(((BinaryDataBuffer)_clientAes.DecryptBin(token.ToByteArray())).Slice(0, 16));
						//_userName = userName;
						Connected = true;
					}
				}
			}
		}

		private DataPackage SendAndRecieve(DataPackage p)
		{
			var socket = SocketPool.Rent(_options.Host, _options.Port);
			var res = SocketPool.SendAndResieve(socket, p.ToArray());
			SocketPool.Return(socket);
			return new DataPackage(res);

		}
	}

	public class TCPConnectorException : Exception
	{
		public string RemoteSource { get; }
		public string RemoteStackTrace { get; }
		public TCPConnectorException(DataPackage remoteErrorPackage)
			: base(GetErrorProperty(remoteErrorPackage, "ErrorDescription"))
		{
			remoteErrorPackage.GoDataTop();
			if (remoteErrorPackage.Read())
			{
				RemoteStackTrace = (string)remoteErrorPackage["StackTrace"];
				RemoteSource = (string)remoteErrorPackage["Source"];
			}
		}
		private static string GetErrorProperty(DataPackage remoteErrorPackage, string key)
		{
			remoteErrorPackage.GoDataTop();
			remoteErrorPackage.Read();
			return (string)remoteErrorPackage[key];
		}
	}

	static class SocketPool
	{
		const int pool_size = 15;
		private static readonly System.Threading.ThreadLocal<object> obj4Lock
			= new System.Threading.ThreadLocal<object>(() => new object());

		private static readonly Socket[] _sockets = new Socket[pool_size];
		private static readonly IntPtr[] _rented = new IntPtr[pool_size];

		public static Socket Rent(string host, int port)
		{
			int i;
			lock (obj4Lock)
			{
				for (i = GetFreeIndex(); i == pool_size; i = GetFreeIndex())
					System.Threading.Thread.Sleep(10);
				_rented[i] = new IntPtr(1);
			}

			if (_sockets[i] == null)
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				_sockets[i] = socket;
			}
			_rented[i] = _sockets[i].Handle;
			return _sockets[i];
		}

		private static int GetFreeIndex()
		{
			int i;
			for (i = 0; i < pool_size; i++)
				if (_rented[i] == IntPtr.Zero)
					break;
			return i;
		}

		public static void Return(Socket socket)
		{
			int i = _rented.IndexOf(socket.Handle);
			_rented[i] = IntPtr.Zero;
		}

		public static void Clear()
		{
			for (int i = 0; i < pool_size; i++)
			{
				var socket = _sockets[i];
				if (socket != null && socket.Connected)
				{
					socket.Close();
					socket.Dispose();
					_sockets[i] = null;
					_rented[i] = IntPtr.Zero;
				}
			}
		}
		public static byte[] SendAndResieve(Socket socket, byte[] data)
		{
			lock (socket)
			{
				socket.Send(BitConverter.GetBytes(data.Length));
				socket.Send(data);

				var buffer = new byte[4];
				socket.Receive(buffer);

				var byteCount = BitConverter.ToInt32(buffer, 0);
				byte[] res = new byte[byteCount];
				socket.Receive(res);
				return res;
			}
		}

	}
}

