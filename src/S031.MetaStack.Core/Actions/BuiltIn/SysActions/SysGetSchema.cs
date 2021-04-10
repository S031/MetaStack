using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;

namespace S031.MetaStack.Core.Actions
{
	internal class SysGetSchema : IAppEvaluator
	{
		string _connectionName;
		string _objectName;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			var ctx = ai.GetContext();

			using (JMXFactory f = ctx.CreateJMXFactory(_connectionName))
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
			var ctx = ai.GetContext();

			using (JMXFactory f = ctx.CreateJMXFactory(_connectionName))
			{
				return ai.GetOutputParamTable()
					.AddNew()
					.SetValue("ObjectSchema", (await f.CreateJMXRepo().GetSchemaAsync(_objectName)).ToString())
					.Update();
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			_connectionName = ai.GetContext().ConnectionName;

			if (dp.Read())
				_objectName = (string)dp["ObjectName"];
		}
	}
}
