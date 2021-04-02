using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using S031.MetaStack.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Services;
using S031.MetaStack.Data;

namespace S031.MetaStack.Services
{
	public class TCPServerService : BackgroundService
	{
		private Socket _listener;
		private CancellationToken _token;
		private readonly ILogger _log;
		private readonly IServiceProvider _services;
		private long _maxReceivedMessageSize = 1048576;
		private readonly HostedServiceOptions _options;
		private static readonly string _nameof = typeof(TCPServerService).FullName;


		async Task Listen()
		{
			while (!_token.IsCancellationRequested)
			{
				await _listener.AcceptAsync()
				.ContinueWith(Accept, _token)
				.ConfigureAwait(false);
			}
		}

		async Task Accept(Task<Socket> task)
		{
			using (var client = await task)
			{
				var buffer = new byte[4];
				while (client.Connected)
				{
					byte[] response;
					int streamSize;
					try
					{
						buffer = await GetByteArrayFromStreamAsync(client, 4);
						streamSize = BitConverter.ToInt32(buffer, 0);
						if (streamSize == 0)
							break;
						else if (streamSize > _maxReceivedMessageSize)
							throw new InvalidOperationException(
								$"The size of the incoming message is greater than the one specified in the settings({_maxReceivedMessageSize})");
						var res = await GetByteArrayFromStreamAsync(client, streamSize);

						using (var dr = await ProcessMessageAsync(new DataPackage(res)))
							response = dr.ToArray();
						//ArrayPool<byte>.Shared.Return(res);
					}
					catch (Exception ex)
					{
						using (var dr = DataPackage.CreateErrorPackage(ex))
							response = dr.ToArray();
					}
					streamSize = response.Length;
					await client.SendAsync(BitConverter.GetBytes(streamSize), SocketFlags.None);
					await client.SendAsync(response, SocketFlags.None);
				}
			}
		}

		private async Task<DataPackage> ProcessMessageAsync(DataPackage inputMessage)
		{
			using (var messagePipeline = new MessagePipeline(_services))
			{
				return await messagePipeline.ProcessMessageAsync(inputMessage, _token);
			}
		}

		private static async Task<byte[]> GetByteArrayFromStreamAsync(Socket socket, int length)
		{
			Memory<byte> result = new Memory<byte>(new byte[length]);
			await socket.ReceiveAsync(result, SocketFlags.None);
			return result.ToArray();
		}

		public override  async Task StopAsync(CancellationToken cancellationToken)
		{
			_listener.Close();
			_log.LogDebug($"{_nameof} successfully stoped");
			(_log as IDisposable)?.Dispose();
			await base.StopAsync(cancellationToken);
		}

		public TCPServerService(IServiceProvider services, HostedServiceOptions options)
		{
			_services = services;
			if (options.LogName.IsEmpty())
				_log = services.GetRequiredService<ILogger>();
			else
				_log = services.GetRequiredService<ILoggerProvider>()
					.CreateLogger(options.LogName);
			_options = options;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_token = stoppingToken;
			_listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
			_maxReceivedMessageSize = _options.Parameters.GetValue<int>("MaxReceivedMessageSize", 1048576);
			_listener.Bind(new IPEndPoint(IPAddress.Loopback, _options.Parameters.GetValue<int>("Port", 8001)));
			_listener.Listen(120);
			_log.LogDebug($"{_nameof} successfully started");
			await Listen();
		}
	}
}