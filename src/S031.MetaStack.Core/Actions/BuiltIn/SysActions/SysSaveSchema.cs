using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.Actions
{
	internal class SysSaveSchema : IAppEvaluator
	{
		ConnectInfo _connectInfo;
		JMXSchema _schemaSource;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			{
				return ai.GetOutputParamTable()
					.AddNew()
					.SetValue("ObjectSchema", (f.CreateJMXRepo().SaveSchema(_schemaSource)).ToString())
					.Update();
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			//using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			using (MdbContext mdb = new MdbContext(_connectInfo))
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			{
				return ai.GetOutputParamTable()
					.AddNew()
					.SetValue("ObjectSchema", (await f.CreateJMXRepo().SaveSchemaAsync(_schemaSource)).ToString())
					.Update();
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			var configuration = ApplicationContext.GetConfiguration();
			if (!dp.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = configuration["appSettings:SysCatConnection"];
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

			if (dp.Read())
				_schemaSource = JMXSchema.Parse((string)dp["ObjectName"]);
		}
	}
}
