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
		object _connectionName;
		string _viewName;
		readonly List<object> _parameters = new List<object>();
		List<JMXCondition> _conditions;


		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (JMXFactory f = ApplicationContext.CreateJMXFactory((string)_connectionName))
			using (JMXRepo repo = (f.CreateJMXRepo() as JMXRepo))
			using (SQLStatementWriter writer = new SQLStatementWriter(repo))
			{
				JMXSchema schema = repo.GetSchema(_viewName);
				string _body = writer.WriteSelectStatement(
					schema,
					_conditions.ToArray())
					.ToString();

				if (schema.DbObjectType == DbObjectTypes.Action)
					using (ActionManager am = new ActionManager(f.GetMdbContext(ContextTypes.SysCat))
					{
						Logger = ApplicationContext.GetLogger()
					})
						return am.Execute(_body, dp);
				else
					return f
						.GetMdbContext(ContextTypes.Work)
						.GetReader(_body, CreateParameters(schema));
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (JMXFactory f = ApplicationContext.CreateJMXFactory((string)_connectionName))
			using (JMXRepo repo = (f.CreateJMXRepo() as JMXRepo))
			using (SQLStatementWriter writer = new SQLStatementWriter(repo))
			{
				JMXSchema schema = await repo.GetSchemaAsync(_viewName);
				string body = writer.WriteSelectStatement(
					schema,
					_conditions.ToArray())
					.ToString();

				if (schema.DbObjectType == DbObjectTypes.Action)
					using (ActionManager am = new ActionManager(f.GetMdbContext(ContextTypes.SysCat))
					{
						Logger = ApplicationContext.GetLogger()
					})
						return await am.ExecuteAsync(body, dp);
				else
					return await f
						.GetMdbContext(ContextTypes.Work)
						.GetReaderAsync(body, CreateParameters(schema));
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			_connectionName = ai.GetContext().ConnectionName;

			_conditions = new List<JMXCondition>();
			for (; dp.Read();)
			{
				string paramName = (string)dp["ParamName"];
				if (paramName[0] == '@')
				{
					_parameters.Add(dp["ParamName"]);
					_parameters.Add(dp["ParamValue"]);
				}
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
		}

		private MdbParameter[] CreateParameters(JMXSchema schema)
		{
			var mdbParams = new List<MdbParameter>();
			foreach (JMXParameter p in schema.Parameters)
			{
				int idx = _parameters.IndexOf(p.ParamName);
				if ((idx < 0 || idx >= _parameters.Count - 1) && p.Required)
					throw new InvalidOperationException(
						$"Обязательный параметер схемы '{schema.ObjectName}.{p.ParamName}', не найден в списке параметров  операции 'SysSelect'");
				else if (idx > -1 && idx < _parameters.Count - 1)
				{
					object value = _parameters[idx + 1];
					Type type = p.DataType.Type();
					mdbParams.Add(new MdbParameter(p.ParamName, value.CastOf(type)) { NullIfEmpty = p.NullIfEmpty });
				}
			}
			return mdbParams.ToArray();
		}
	}
}
