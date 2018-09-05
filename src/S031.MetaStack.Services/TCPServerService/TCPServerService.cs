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
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Services;

namespace S031.MetaStack.Services
{
	public class TCPServerService : BackgroundService
	{
		private TcpListener _listener;
		private CancellationToken _token;
		private ILogger _log;
		private long _maxReceivedMessageSize = 1048576;
		private readonly HostedServiceOptions _options;
		private static readonly string _nameof = typeof(TCPServerService).FullName;


		async Task Listen()
		{
			while (!_token.IsCancellationRequested)
			{
				await _listener.AcceptTcpClientAsync()
				.ContinueWith(Accept, _token)
				.ConfigureAwait(false);
			}
		}
		async Task Accept(Task<TcpClient> task)
		{
			using (var client = task.Result)
			using (var stream = client.GetStream())
			{
				var buffer = new byte[4];
				while (client.Connected)
				{
					byte[] response;
					int streamSize;
					try
					{
						buffer = await GetByteArrayFromStreamAsync(stream, 4);
						streamSize = BitConverter.ToInt32(buffer, 0);
						if (streamSize == 0)
							break;
						else if (streamSize > _maxReceivedMessageSize)
							throw new InvalidOperationException(
								$"The size of the incoming message is greater than the one specified in the settings({_maxReceivedMessageSize})");
						var res = await GetByteArrayFromStreamAsync(stream, streamSize);

						response = (await ProcessMessage(new DataPackage(res))).ToArray();
					}
					catch (Exception ex)
					{
						response = DataPackage.CreateErrorPackage(ex).ToArray();
					}
					streamSize = response.Length;
					await stream.WriteAsync(BitConverter.GetBytes(streamSize), 0, 4);
					await stream.WriteAsync(response, 0, streamSize);
				}
			}
		}

		static async Task<DataPackage> ProcessMessage(DataPackage inputMessage)
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

		public override  async Task StopAsync(CancellationToken cancellationToken)
		{
			_listener.Stop();
			_log.Debug($"{_nameof} successfully stoped");
			(_log as FileLogger)?.Dispose();
			await base.StopAsync(cancellationToken);
		}


		public TCPServerService(HostedServiceOptions options)
		{
			var serviceBuilder = ApplicationContext.GetServices(new ServiceProviderOptions() { ValidateScopes = true });
			using (var scope = serviceBuilder.CreateScope())
			{
				var serviceProvider = scope.ServiceProvider;
				if (!options.LogName.IsEmpty())
					_log = new FileLogger(_nameof, options.LogSettings);
				else
					_log = serviceProvider.GetRequiredService<ILogger>();
			}
			_options = options;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_token = stoppingToken;
			_listener = TcpListener.Create(_options.Parameters.GetValue<int>("Port", 8001));
			_maxReceivedMessageSize = _options.Parameters.GetValue<int>("MaxReceivedMessageSize", 1048576);
			_listener.Start();
			_log.Debug($"{_nameof} successfully started");
			await Listen();
		}
	}
}