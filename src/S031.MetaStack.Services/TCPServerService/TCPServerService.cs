using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S031.MetaStack.Core.App;
using S031.MetaStack.Common;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Services
{
	//public class TCPServerService : IAppService
	//{
	//	object _lock = new Object(); // sync lock 
	//	List<Task> _connections = new List<Task>(); // pending connections
	//	public bool IsRuning()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public Task StartAsync(object host, AppServiceOptions options)
	//	{
	//		return Task.Run(async () =>
	//		{
	//			var tcpListener = TcpListener.Create(8001);
	//			tcpListener.Start();
	//			while (true)
	//			{
	//				var tcpClient = await tcpListener.AcceptTcpClientAsync();
	//				var task = StartHandleConnectionAsync(tcpClient);
	//				// if already faulted, re-throw any error on the calling context
	//				if (task.IsFaulted)
	//					task.Wait();
	//			}
	//		});
	//	}
	//	private async Task StartHandleConnectionAsync(TcpClient tcpClient)
	//	{
	//		// start the new connection task
	//		var connectionTask = HandleConnectionAsync(tcpClient);

	//		// add it to the list of pending task 
	//		lock (_lock)
	//			_connections.Add(connectionTask);

	//		// catch all errors of HandleConnectionAsync
	//		try
	//		{
	//			await connectionTask;
	//			// we may be on another thread after "await"
	//		}
	//		catch (Exception ex)
	//		{
	//			// log the error
	//			Console.WriteLine(ex.ToString());
	//		}
	//		finally
	//		{
	//			// remove pending task
	//			lock (_lock)
	//				_connections.Remove(connectionTask);
	//		}
	//	}
	//	private static Task HandleConnectionAsync(TcpClient tcpClient)
	//	{
	//		return Task.Run(async () =>
	//		{
	//			using (var networkStream = tcpClient.GetStream())
	//			{
	//				var buffer = new byte[4096];
	//				//Console.WriteLine("[Server] Reading from client");
	//				var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
	//				var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
	//				//Console.WriteLine("[Server] Client wrote {0}", request);
	//				var serverResponseBytes = Encoding.UTF8.GetBytes("Hello from server");
	//				await networkStream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
	//				//Console.WriteLine("[Server] Response has been written");
	//			}
	//		});
	//	}

	//	public Task StopAsync()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}

	public class TCPServerService : IAppService
	{
		private TcpListener _listener;
		private CancellationToken _token;
		private ILogger _log;
		private bool isLocalLog;
		private IAppConfig _config;
		private IAppHost _host;
		private readonly string _nameof = typeof(TCPServerService).FullName;

		public async Task StartAsync(IAppHost host, AppServiceOptions options)
		{
			_host = host;
			_token = options.CancellationToken == CancellationToken.None ? new CancellationTokenSource().Token :
				options.CancellationToken;
			_config = host.AppConfig;
			if (options.LogName.IsEmpty())
				_log = host.Log;
			else
			{
				_log = new FileLogger(_nameof, options.LogSettings);
				isLocalLog = true;
			}
			_listener = new TcpListener(IPAddress.Any, 8001);
			_listener.Start();
			_log.Debug($"AppService {_nameof} successfully started");
			// Require new thread for non blocked run
			var t = await Task.Factory.StartNew(async () => await Listen());
		}

		async Task Listen()
		{
			var client = default(TcpClient);
			while (!_token.IsCancellationRequested)
			{
				try
				{
					client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
				}
				catch (ObjectDisposedException)
				{
					// The listener has been stopped.
					return;
				}

				if (client == null) return;
				await Accept(client);
			}
		}
		async Task Accept(TcpClient client)
		{
			using (client)
			{
				var stream = client.GetStream();
				DataPackage p;
				bool closeChannel = false;
				try
				{
					var r = await getMessage(stream);
					p = r.dataPackage;
					closeChannel = r.closeChannel;
					//_log.LogDebug($"Accept Message: {p.ToString(TsExportFormat.JSON)}");
					p = await processMessage(p);
				}
				catch (Exception e)
				{
					p = DataPackage.CreateErrorPackage(e);
				}
				byte[] data = p.ToArray();
				await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
				await stream.WriteAsync(data, 0, data.Length);

				// If ReadAsync returns zero, it means the connection was closed from the other side. If it doesn't, we have to close it ourselves.
				//if (bytesRead != 0) client.Close(); // Do a graceful shutdown
				if (closeChannel) client.Close();
			}
		}
		async Task<DataPackage> processMessage(DataPackage inputMessage)
		{
			var p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
			p.Headers.Add("Username", "Сергей");
			p.Headers.Add("Password", "1234567T");
			p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
			p.UpdateHeaders();
			int i = 0;
			for (i = 0; i < 1000; i++)
			{
				p.AddNew();
				p["Col1"] = i;
				p["Col2"] = $"Строка # {i}";
				p["Col3"] = DateTime.Now.AddDays(i);
				p["Col4"] = Guid.NewGuid();
				//без сериализации работает в 1.5 раза быстрееp
				p["Col5"] = null;
				//p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
				p.Update();
			}
			return p;
		}
		async Task<(DataPackage dataPackage, bool closeChannel)> getMessage(NetworkStream stream)
		{
			byte[] buffer = new byte[4096];
			var bytesRead = 0;

			// First, we need to know how much data to read. We've got a 4-byte fixed-size header to handle that.
			// It's unlikely we'd read the header in multiple ReadAsync calls (it's only 4 bytes :)), but it's good practice anyway.
			var headerRead = 0;
			while (headerRead < 4 && (bytesRead = await stream.ReadAsync(buffer, headerRead, 4 - headerRead).ConfigureAwait(false)) > 0)
			{
				headerRead += bytesRead;
			}

			if (headerRead < 4)
				// the minimum message length can not be less than 32 bytes
				throw new FormatException(Translater.GetString("S031.MetaStack.Services.TCPServerService.Accept.1"));

			var bytesRemaining = BitConverter.ToInt32(buffer, 0);
			if (bytesRemaining < 32)
				// the minimum message length can not be less than 32 bytes
				throw new FormatException(Translater.GetString("S031.MetaStack.Services.TCPServerService.Accept.1"));

			int totalBytes = bytesRemaining;

			List<byte> l = new List<byte>();
			while (bytesRemaining > 0 && (bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(bytesRemaining, buffer.Length))) != 0)
			{
				//l.AddRange(buffer);
				for (int i = 0; i < bytesRead; i++)
				{
					l.Add(buffer[i]);
				}
				bytesRemaining -= bytesRead;
			}
			return (new DataPackage(l.ToArray()), (bytesRead != 0) );
		}

		public async Task StopAsync()
		{
			await Task.Factory.StartNew(() => Stop());
		}

		public void Stop()
		{
			_listener.Stop();
			_log.Debug($"AppService {_nameof} successfully stoped");
			if (isLocalLog)
				(_log as IDisposable)?.Dispose();
		}

		public bool IsRuning()
		{
			throw new NotImplementedException();
		}
	}
}