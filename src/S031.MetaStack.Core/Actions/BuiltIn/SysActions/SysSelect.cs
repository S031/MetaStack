using System;
using System.Collections.Generic;
using System.Linq;
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
	internal class SysSelect : IAppEvaluator
	{
		readonly List<object> _parameters = new List<object>();
		ConnectInfo _connectInfo;
		JMXSchema _schema;
		List<JMXCondition> _conditions;
		string _viewName;
		string _body;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			{
				//Костыль!!!
				CreateCommandAsync().GetAwaiter().GetResult();
				return mdb.GetReader(_body, _parameters.ToArray());
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			//using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			using (MdbContext mdb = new MdbContext(_connectInfo))
			{
				await CreateCommandAsync();
				if (_schema.DbObjectType == DbObjectTypes.Action)
					using (ActionManager am = new ActionManager(mdb) { Logger = ApplicationContext.GetLogger() })
						return await am.ExecuteAsync(_body, dp);
				else
					return await mdb.GetReaderAsync(_body, _parameters.ToArray());
			}
		}
		/// <summary>
		/// Костыль!!! Check parameters with object schema (aka action execute)
		/// with NullIfEmpty 
		/// </summary>
		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			if (!dp.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = ApplicationContext.GetConfiguration()["appSettings:defaultConnection"]; ;

			_conditions = new List<JMXCondition>();
			for (; dp.Read();)
			{
				string paramName = (string)dp["ParamName"];
				if (paramName[0] == '@')
				{
					_parameters.Add(dp["ParamName"]);
					_parameters.Add(dp["ParamValue"]);
				}
				else if (paramName.Equals("_connectionName", StringComparison.CurrentCultureIgnoreCase))
					connectionName = (string)dp["ParamValue"];
				else if (paramName.Equals("_viewName", StringComparison.CurrentCultureIgnoreCase))
					_viewName = (string)dp["ParamValue"];
				else if (paramName.Equals("_filter", StringComparison.CurrentCultureIgnoreCase))
					_conditions.Add(new JMXCondition(JMXConditionTypes.Where, (string)dp["ParamValue"]));
				else if (paramName.Equals("_sort", StringComparison.CurrentCultureIgnoreCase))
					_conditions.Add(new JMXCondition(JMXConditionTypes.OrderBy, (string)dp["ParamValue"]));
				else if (paramName.Equals("_group", StringComparison.CurrentCultureIgnoreCase))
					_conditions.Add(new JMXCondition(JMXConditionTypes.GroupBy, (string)dp["ParamValue"]));
				else if (paramName.Equals("_having", StringComparison.CurrentCultureIgnoreCase))
					_conditions.Add(new JMXCondition(JMXConditionTypes.Havind, (string)dp["ParamValue"]));
				else if (paramName.Equals("_join", StringComparison.CurrentCultureIgnoreCase))
					_conditions.Add(new JMXCondition(JMXConditionTypes.Join, (string)dp["ParamValue"]));
			}
			var configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

		}
		private async Task CreateCommandAsync()
		{
			var config = ApplicationContext.GetConfiguration();
			var connectInfo = config.GetSection($"connectionStrings:{config["appSettings:SysCatConnection"]}").Get<ConnectInfo>();

			//using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(connectInfo))
			using (MdbContext mdb = new MdbContext(connectInfo))
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			using (JMXRepo repo = (f.CreateJMXRepo() as JMXRepo))
			using (SQLStatementWriter writer = new SQLStatementWriter(repo))
			{
				_schema = await repo.GetSchemaAsync(_viewName);
				_body = writer.WriteSelectStatement(
					_schema,
					_conditions.ToArray())
					.ToString();
			}
		}
	}
}
