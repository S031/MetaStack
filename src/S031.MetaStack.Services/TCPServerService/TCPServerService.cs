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
		private readonly ILogger _log;
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
			using (var client = await task)
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
					await stream.WriteAsync(BitConverter.GetBytes(streamSize), 0, 4);
					await stream.WriteAsync(response, 0, streamSize);
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

		private static async Task<byte[]> GetByteArrayFromStreamAsync(NetworkStream ns, int length)
		{
			Memory<byte> result = new Memory<byte>(new byte[length]);
			await ns.ReadAsync(result);
			//byte[] result = new byte[length];
			////byte[] result = ArrayPool<byte>.Shared.Rent(length);
			//int ReadBytes = 0;
			//while (length > ReadBytes)
			//{
			//	ReadBytes += await ns.ReadAsync(result, ReadBytes, length - ReadBytes);
			//	if (ReadBytes == 0)
			//		break;
			//}
			return result.ToArray();
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