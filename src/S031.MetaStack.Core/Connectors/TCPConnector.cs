using S031.MetaStack.Core.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using S031.MetaStack.Core.Data;
using System.Security.Cryptography;
using S031.MetaStack.Core.Security;
#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace S031.MetaStack.Core.Connectors
#else

namespace S031.MetaStack.WinForms.Connectors
#endif
{
	public class TCPConnector:IDisposable
	{
		const string _endPointConfigName = "TCPConnector";

		private Socket _socket;
		private NetworkStream  _stream;
		private bool _connected = false;

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

		public void Connect(string userName, string password)
		{
			if (_socket == null || !_socket.Connected)
				InitSocket(_endPointConfigName);
			if (!_connected)
				Connecting(userName, password);
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
}
