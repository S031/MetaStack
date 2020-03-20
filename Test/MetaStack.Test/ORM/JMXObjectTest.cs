//using Newtonsoft.Json;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.ORM;
using System.Text;
using Xunit;

namespace MetaStack.Test.ORM
{
	public class ORMObjectTest
	{
		private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Test.Resources.TestData.TestJson);
		private readonly string _connectionName;
		public ORMObjectTest()
		{
			Program.ConfigureTests();
			_connectionName = ApplicationContext
				.GetConfiguration()["appSettings:defaultConnection"];
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		private void JMXObjectReadTest()
		{
			using (JMXFactory f = ApplicationContext.CreateJMXFactory(_connectionName))
			{
				var p = f.CreateJMXProvider();
				var o = p.Read("DealAcc", 12345);

			}
		}
	}
}
