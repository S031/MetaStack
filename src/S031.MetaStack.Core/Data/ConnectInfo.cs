using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace S031.MetaStack.Core.Data
{
	public class ConnectInfo
	{
		private static readonly ConcurrentDictionary<string, ConnectInfo> _csCache = new ConcurrentDictionary<string, ConnectInfo>();

		public const string ProviderNameField = "Provider Name";
		public const string ProviderNameDefault = "System.Data.SqlClient";
		public const string DbNameField = "Initial Catalog";

		public ConnectInfo(string connectionString)
		{
			if (_csCache.TryGetValue(connectionString, out ConnectInfo connectInfo))
			{
				ConnectionString = connectInfo.ConnectionString;
				ProviderName = connectInfo.ProviderName;
				DbName = connectInfo.DbName;
			}
			else
			{
				DbConnectionStringBuilder sb = new DbConnectionStringBuilder
				{
					ConnectionString = connectionString
				};
				if (!sb.ContainsKey(ProviderNameField))
				{
					ProviderName = ProviderNameDefault;
					ConnectionString = connectionString;
				}
				else
				{
					ProviderName = (string)sb[ProviderNameField];
					sb.Remove(ProviderNameField);
					ConnectionString = sb.ToString();
				}
				if (sb.ContainsKey(DbNameField))
					DbName = (string)sb[DbNameField];
				if (!_csCache.ContainsKey(connectionString))
					_csCache.TryAdd(connectionString, this);
			}

		}

		public ConnectInfo(string providerName, string connectionString)
			: this($"{ProviderNameField}={providerName};{connectionString}")
		{
		}

		public string ProviderName { get; set; }
		public string DbName { get; }
		public string ConnectionString { get; set; }

		public override string ToString() =>
			$"{ProviderNameField}={ProviderName};{ConnectionString}";

		public override bool Equals(object obj) => this.ToString().Equals(obj.ToString(), StringComparison.CurrentCultureIgnoreCase);

		public override int GetHashCode() =>
			new { ProviderName, ConnectionString }.GetHashCode();

		public static implicit operator ConnectInfo(string s) => new ConnectInfo(s);

		public static implicit operator string(ConnectInfo connectInfo) => connectInfo.ToString();
	}
}

