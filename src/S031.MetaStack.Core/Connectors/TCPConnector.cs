using S031.MetaStack.Core.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
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

		public static TCPConnector Create() => new TCPConnector(_endPointConfigName);

		TCPConnector(string endPointConfigName)
		{
#if NETCOREAPP
			var config = ApplicationContext.GetServices().GetService<IConfiguration>().GetSection($"Connectors:{endPointConfigName}");
			var host = config.GetValue<string>("Host");
			var port = config.GetValue<int>("Port");
#else

#endif
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(host, port);
			_stream = new NetworkStream(_socket);
		}

		public void Dispose()
		{
			_stream.Close();
			_socket.Close();
			_stream.Dispose();
			_socket.Dispose();
		}


	}
}
