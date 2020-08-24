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

		public MdbContext GetContext(string connectionName)
		{
			var connectInfoSection = _config.GetSection($"connectionStrings:{connectionName}");
			return new MdbContext();
		}
	}
}
