using System;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using S031.MetaStack.Common;
using System.Linq;
#if NETCOREAPP
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace S031.MetaStack.Core.Connectors
#else
using S031.MetaStack.WinForms.Data;
using S031.MetaStack.WinForms.Security;
using S031.MetaStack.Json;

namespace S031.MetaStack.WinForms.Connectors
#endif
{
	public class TCPConnector : IDisposable
	{
		const string _endPointConfigName = "TCPConnector";
		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;

		//private Socket _socket;
		//private NetworkStream _stream;
		private readonly string _host;
		private readonly int _port;
		//Connection info
		private bool _connected = false;
		private string _userName;
		private Guid _ticket;
		private Aes _clientAes;
		private LoginInfo _loginInfo;

		public static TCPConnector Create() => new TCPConnector(_endPointConfigName);

		TCPConnector(string endPointConfigName)
		{
#if NETCOREAPP
			var config = ApplicationContext
				.GetServices()
				.GetService<IConfiguration>()
				.GetSection($"Connectors:{endPointConfigName}");
			_host = config.GetValue<string>("Host");
			_port = config.GetValue<int>("Port");
#else
			string setting = System.Configuration.ConfigurationManager.AppSettings["TCPConnector"].Replace('\'', '"');
			var j = new JsonReader(ref setting)
				.Read();
			if (j != null)
			{
				_host = (string)j["Host"];
				_port = (int)j["Port"];
			}
#endif
			InitSocket();
		}

		private void InitSocket()
		{
			//_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//_socket.Connect(_host, _port);
			//_stream = new NetworkStream(_socket);
			_connected = false;
		}

		public bool Connected => _connected; // _socket != null && _socket.Connected && _connected;

		public void Dispose()
		{
			Disconnect();
			_clientAes?.Dispose();
			//_stream.Close();
			//_socket.Close();
			//_stream.Dispose();
			//_socket.Dispose();
		}

		public TCPConnector Connect(string userName, string password)
		{
			//if (_socket == null || !_socket.Connected)
			//	InitSocket();
			if (!_connected)
				Connecting(userName, password);
			return this;
		}
		private void Disconnect()
		{
			//if (_socket == null || !_socket.Connected)
			//	return;
			if (!_connected)
				return;
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
			paramTable.Headers["ActionID"] = actionID;
			paramTable.Headers["UserName"] = _userName;
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
						_ticket = new Guid(_clientAes.DecryptBin(token.ToByteArray()).Take(16).ToArray());
						_userName = userName;
						_connected = true;
					}
				}
			}
		}

		private DataPackage SendAndRecieve(DataPackage p)
		{
			using (var socket = CreateSocket(_host, _port))
			using (var stream = new NetworkStream(socket))
			{
				var data = p.ToArray();
				stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
				stream.Write(data, 0, data.Length);

				var buffer = new byte[4];
				int ReadBytes = 0;
				while (4 > ReadBytes)
				{
					ReadBytes += stream.Read(buffer, ReadBytes, 4 - ReadBytes);
					if (ReadBytes == 0)
						break;
				}

				var byteCount = BitConverter.ToInt32(buffer, 0);
				var res = new byte[byteCount];
				ReadBytes = 0;
				while (byteCount > ReadBytes)
				{
					ReadBytes += stream.Read(res, ReadBytes, byteCount - ReadBytes);
					if (ReadBytes == 0)
						break;
				}
				socket.Shutdown(SocketShutdown.Both);
				socket.Disconnect(false);
				socket.Close();
				return new DataPackage(res);
			}
		}
		private static Socket CreateSocket(string host, int port)
		{
			var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(host, port);
			return _socket;
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
}
