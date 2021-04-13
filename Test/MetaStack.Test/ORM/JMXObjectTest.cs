//using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using System.Text;
using Xunit;

namespace MetaStack.Test.ORM
{
	public class ORMObjectTest
	{
		//private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Test.Resources.TestData.TestJson);
		private readonly string _connectionName;
		private readonly ILogger _logger;
		public ORMObjectTest()
		{
			var s = Program.GetServices();
			_connectionName = s.GetRequiredService<IConfiguration>()["appSettings:defaultConnection"];
			_logger = s.GetRequiredService<ILoggerProvider>().CreateLogger("ORMObjectTest");
		}

		[Fact]
		public void JMXObjectReadTest()
		{
			var mdbFactory = Program.GetServices().GetRequiredService<MdbContextFactory>();
			using (JMXFactory f = JMXFactory.Create(mdbFactory.GetContext(_connectionName), _logger))
			{
				var p = f.CreateJMXProvider();
				var o = p.Read("DealAcc", 12345);

			}
		}
	}
}
