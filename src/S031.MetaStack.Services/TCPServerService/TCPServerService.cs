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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Core.Services;

namespace S031.MetaStack.Services
{
	public class TCPServerService : BackgroundService, IAppService
	{
		private TcpListener _listener;
		private CancellationToken _token;
		private ILogger _log;
		private HostedServiceOptions _options;
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
			_listener = TcpListener.Create(8001);
			_listener.Start();
			await listen(_listener, _token);
			_log.Debug($"AppService {_nameof} successfully started");
		}


		static async Task listen(TcpListener listener, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await listener.AcceptTcpClientAsync()
				.ContinueWith(Accept, token)
				.ConfigureAwait(false);
			}
		}
		static async Task Accept(Task<TcpClient> task)
		{
			using (var client = task.Result)
			using (var stream = client.GetStream())
			{
				var buffer = new byte[4];
				while (client.Connected)
				{
					buffer = await GetByteArrayFromStreamAsync(stream, 4);
					var streamSize = BitConverter.ToInt32(buffer, 0);
					if (streamSize == 0)
						break;
					var res = await GetByteArrayFromStreamAsync(stream, streamSize);

					var response = (await processMessage(new DataPackage(res))).ToArray();
					streamSize = response.Length;
					await stream.WriteAsync(BitConverter.GetBytes(streamSize), 0, 4);
					await stream.WriteAsync(response, 0, streamSize);
				}
			}
		}

		static async Task<DataPackage> processMessage(DataPackage inputMessage)
		{
			return await Task.Factory.StartNew(() =>
			{
				var p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
				p.UpdateHeaders();
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					p["Col5"] = null;
					p.Update();
				}
				return p;
			});
		}

		private static async Task<byte[]> GetByteArrayFromStreamAsync(NetworkStream ns, int length)
		{
			byte[] result = new byte[length];
			int ReadBytes = 0;
			while (length > ReadBytes)
				ReadBytes += await ns.ReadAsync(result, ReadBytes, length - ReadBytes);
			return result;
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
		public TCPServerService(ILogger log, HostedServiceOptions options)
		{
			_log = log;
			_options = options;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_token = stoppingToken;
			_listener = TcpListener.Create(8001);
			_listener.Start();
			_log.Debug($"AppService {_nameof} successfully started");
			await listen(_listener, _token);
		}
		public override void Dispose()
		{
			Stop();
			base.Dispose();
		}
	}
}