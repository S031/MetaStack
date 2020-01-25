using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using S031.MetaStack.Core.App;
using S031.MetaStack.Common;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Logging;
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

		static async Task<DataPackage> ProcessMessageAsync(DataPackage inputMessage)
		{
			using (var messagePipeline = new MessagePipeline(inputMessage))
			{
				await messagePipeline.ProcessMessageAsync();
				return messagePipeline.ResultMessage;
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
			_listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
			_maxReceivedMessageSize = _options.Parameters.GetValue<int>("MaxReceivedMessageSize", 1048576);
			_listener.Bind(new IPEndPoint(IPAddress.Loopback, _options.Parameters.GetValue<int>("Port", 8001)));
			_listener.Listen(120);
			_log.Debug($"{_nameof} successfully started");
			await Listen();
		}
	}
}