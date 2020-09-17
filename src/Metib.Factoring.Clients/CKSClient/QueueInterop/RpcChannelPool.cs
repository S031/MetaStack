using Metib.Factoring.Clients.CKS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metib.Factoring.Clients.CKS
{
	internal static class RpcChannelPool
	{
		private const int _connection_pool_size = 10;

		private static readonly RpcChannel[] _rpcChannels = new RpcChannel[_connection_pool_size];
		private static readonly int[] _rented = new int[_connection_pool_size];
		private static readonly bool[] _blocked = new bool[_connection_pool_size];
		private static readonly object obj4Lock = new object();

		public static RpcChannel Rent(string rpcChannelCreationOptions)
			=> _rpcChannels[GetNextFreeAndLock(rpcChannelCreationOptions)];

		public static void Return(RpcChannel channel)
		{
			lock (obj4Lock)
				_rented[channel.PoolPosition]--;
		}

		private static int GetNextFreeAndLock(string rpcChannelCreationOptions)
		{
			int i = -1;
			lock (obj4Lock)
			{
				int? pos = Enumerable.Range(0, _connection_pool_size - 1)
					.Where(i => !_blocked[i])
					.Min(i => _rented[i]);
				if (pos == null)
					throw new InvalidOperationException("Internal error: rabbit connection pool is locked");
				i = pos.Value;
				_blocked[i] = true;
			}
			
			var channel = _rpcChannels[i];
			if (channel == null
				|| channel.Closed)
			{
				channel = new RpcChannel(rpcChannelCreationOptions, i);
				_rpcChannels[i] = channel;
			}

			lock (obj4Lock)
			{
				_rented[i]++;
				_blocked[i] = false;
			}
			return i;
		}
	}
}
