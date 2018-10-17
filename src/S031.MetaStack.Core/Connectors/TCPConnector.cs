using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
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

namespace S031.MetaStack.WinForms.Connectors
#endif
{
	public class TCPConnector:IDisposable
	{
		const string _endPointConfigName = "TCPConnector";
		static readonly RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA256;

		private Socket _socket;
		private NetworkStream  _stream;
		
		//Connection info
		private bool _connected = false;
		private string _userName;
		private Guid _ticket;
		private Aes _clientAes;
		private LoginInfo _loginInfo;

		public static TCPConnector Create() => new TCPConnector(_endPointConfigName);

		TCPConnector(string endPointConfigName)
		{
			InitSocket(endPointConfigName);
		}

		private void InitSocket(string endPointConfigName)
		{
#if NETCOREAPP
			var config = ApplicationContext.GetServices().GetService<IConfiguration>().GetSection($"Connectors:{endPointConfigName}");
			var host = config.GetValue<string>("Host");
			var port = config.GetValue<int>("Port");
#else
			var host = "localhost";
			var port = 8001;
#endif
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(host, port);
			_stream = new NetworkStream(_socket);
			_connected = false;
		}

		public void Dispose()
		{
			_stream.Close();
			_socket.Close();
			_stream.Dispose();
			_socket.Dispose();
		}

		public TCPConnector Connect(string userName, string password)
		{
			if (_socket == null || !_socket.Connected)
				InitSocket(_endPointConfigName);
			if (!_connected)
				Connecting(userName, password);
			return this;
		}
		public DataPackage Execute(string actionID, DataPackage paramTable)
		{
			//for paranoya mode
			//var request = new DataPackage(paramTable.ToArray());
			paramTable.Headers["ActionID"] = actionID;
			paramTable.Headers["UserName"] = _userName;
			paramTable.Headers["SessionID"] = _loginInfo.SessionID.ToString();
			paramTable.Headers["EncryptedKey"] = _clientAes
					.EncryptBin(_ticket
					.ToByteArray()
					.Concat(BitConverter.GetBytes(DateTime.Now.Millisecond)).ToArray())
					.ToBASE64String();
			paramTable.UpdateHeaders();
			var response = SendAndRecieve(_stream, paramTable);
			if (response.Headers.ContainsKey("Status") &&
				(string)response.Headers["Status"] == "ERROR")
				throw new TCPConnectorException(response);
			return response;
		}

		private void Connecting(string userName, string password)
		{
			var clientRSA = RSA.Create();
			var clientPK = clientRSA.Export();
			DataPackage request = new DataPackage("UserName.String.32", "PublicKey.String.256");
			request.Headers.Add("ActionID", "Sys.LoginRequest");
			request.Headers.Add("UserName", userName);
			request.UpdateHeaders();
			request.AddNew();
			request["UserName"] = userName;
			request["PublicKey"] = clientPK;
			request.Update();
			var response = SendAndRecieve(_stream, request);
			if (response.Headers.ContainsKey("Status") &&
				(string)response.Headers["Status"] == "ERROR")
				throw new TCPConnectorException(response);

			response.GoDataTop();
			response.Read();
			_loginInfo = new LoginInfo();
			_loginInfo.Import(clientRSA.Decrypt(((string)response["LoginInfo"]).ToByteArray(), _padding));
			_clientAes = Aes.Create()
				.ImportBin(_loginInfo.CryptoKey);

			request = new DataPackage("UserName.String.32", "SessionID.String.34", "EncryptedKey.String.256");
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
			response = SendAndRecieve(_stream, request);
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

		private static DataPackage SendAndRecieve(NetworkStream stream, DataPackage p)
		{
			var data = p.ToArray();
			stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
			stream.Write(data, 0, data.Length);
			var buffer = new byte[4];

			stream.Read(buffer, 0, 4);
			var byteCount = BitConverter.ToInt32(buffer, 0);
			var res = new byte[byteCount];
			stream.Read(res, 0, byteCount);
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
			return (string)remoteErrorPackage["ErrorDescription"];
		}
	}
}
