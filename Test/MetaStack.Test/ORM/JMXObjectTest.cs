//using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using System;
using System.Text;
using Xunit;

namespace MetaStack.Test.ORM
{
	public class ORMObjectTest
	{
		private readonly string _connectionName;
		private readonly ILogger _logger;
		private readonly IServiceProvider _services;

		public ORMObjectTest()
		{
			_services = Program.GetServices();
			_connectionName = _services.GetRequiredService<IConfiguration>()["appSettings:defaultConnection"];
			_logger = _services.GetRequiredService<ILoggerProvider>().CreateLogger("ORMObjectTest");
		}

		[Fact]
		public void JMXObjectReadTest()
		{
			var mdbFactory = Program.GetServices().GetRequiredService<MdbContextFactory>();
			JMXFactory f = JMXFactory.Create(_services, mdbFactory.GetContext(_connectionName));
			var p = f.CreateJMXProvider();
			var o = p.Read("DealAcc", 12345);
		}
	}
}
