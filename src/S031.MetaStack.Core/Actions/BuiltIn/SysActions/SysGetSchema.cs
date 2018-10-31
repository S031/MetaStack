using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.ORM;

namespace S031.MetaStack.Core.Actions
{
	internal class SysGetSchema : IAppEvaluator
	{
		ConnectInfo _connectInfo;
		string _objectName;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			{
				return ai.GetOutputParamTable()
					.AddNew()
					.SetValue("ObjectSchema", (f.CreateJMXRepo().GetSchema(_objectName)).ToString())
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
					.SetValue("ObjectSchema", (await f.CreateJMXRepo().GetSchemaAsync(_objectName)).ToString())
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
				_objectName = (string)dp["ObjectName"];
		}
	}
}
