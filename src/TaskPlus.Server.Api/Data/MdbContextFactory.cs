using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlus.Server.Data
{
	public class MdbContextFactory : IMdbContextFactory
	{
		private readonly IServiceProvider _services;
		private readonly IConfiguration _config;
		private readonly ILogger _logger;

		public MdbContextFactory(IServiceProvider services)
		{
			_services = services;
			_config = services.GetRequiredService<IConfiguration>();
			_logger = services.GetRequiredService<ILogger>();

		}

		/// <summary>
		/// Return <see cref="MdbContext" with <see cref="ConnectInfo"/> getted from configuration />
		/// !!!Need for test
		/// </summary>
		/// <param name="connectionName">appSettings:{connectionName}? if not exists then connectionStrings:{connectionName}</param>
		/// <returns></returns>
		public MdbContext GetContext(string connectionName)
		{
			var c = _config.GetValue<string>("appSettings:{connectionName}");
			if (!string.IsNullOrEmpty(c))
				connectionName = c;

			var connectInfoSection = _config.GetSection($"connectionStrings:{connectionName}");
			if (!connectInfoSection.Exists())
				throw new KeyNotFoundException(nameof(connectionName));

			ConnectInfo cinfo = connectInfoSection.GetSection("providerName").Exists()
				? new ConnectInfo(connectInfoSection.GetValue<string>("providerName"), connectInfoSection.GetValue<string>("connectionString"))
				: new ConnectInfo(connectInfoSection.GetValue<string>("connectionString"));

			return new MdbContext(cinfo);
		}
	}
}
