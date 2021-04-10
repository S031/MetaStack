using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using S031.MetaStack.Actions;

namespace S031.MetaStack.Core.Actions
{
	internal class SysSaveSchema : IAppEvaluator
	{
		string _connectionName;
		JMXSchema _schemaSource;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			var ctx = ai.GetContext();

			using (JMXFactory f = ctx.CreateJMXFactory(_connectionName))
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
			var ctx = ai.GetContext();

			using (JMXFactory f = ctx.CreateJMXFactory(_connectionName))
			{
				return ai.GetOutputParamTable()
					.AddNew()
					.SetValue("ObjectSchema", (await f.CreateJMXRepo().SaveSchemaAsync(_schemaSource)).ToString())
					.Update();
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			_connectionName = ai.GetContext().ConnectionName;

			if (dp.Read())
				_schemaSource = JMXSchema.Parse((string)dp["ObjectName"]);
		}
	}
}
