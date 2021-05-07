using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using System.Threading.Tasks;

namespace S031.MetaStack.AppServer.Actions
{
	internal class SysGetSchema : IAppEvaluator
	{
		string _connectionName;
		string _objectName;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			var ctx = ai.GetContext();

			var r = ctx.CreateJMXFactory(_connectionName)
				.SchemaFactory
				.CreateJMXRepo();
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("ObjectSchema", (r.GetSchema(_objectName)).ToString())
				.Update();
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			var ctx = ai.GetContext();

			var r = ctx.CreateJMXFactory(_connectionName)
				.SchemaFactory
				.CreateJMXRepo();
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("ObjectSchema", (await r.GetSchemaAsync(_objectName)).ToString())
				.Update();
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			_connectionName = ai.GetContext()
				.Services
				.GetRequiredService<IConfiguration>()["appSettings:SysCatConnection"];

			if (dp.Read())
				_objectName = (string)dp["ObjectName"];
		}
	}
}
