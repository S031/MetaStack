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
		string _viewName;
		string _filter;
		string _sort;
		string _body;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			{
				//Костыль
				CreateCommandAsync(mdb).GetAwaiter().GetResult();
				return mdb.GetReader(_body, _parameters.ToArray());
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			{
				await CreateCommandAsync(mdb);
				if (_schema.DbObjectType == DbObjectTypes.Action)
					using (ActionManager am = new ActionManager(mdb) { Logger = ApplicationContext.GetLogger() })
						return await am.ExecuteAsync(_body, dp);
				else
					return await mdb.GetReaderAsync(_body, _parameters.ToArray());
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			if (!dp.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = "BankLocal";

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
					_filter = (string)dp["ParamValue"];
				else if (paramName.Equals("_sort", StringComparison.CurrentCultureIgnoreCase))
					_sort = (string)dp["ParamValue"];
			}
			var configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

		}
		private async Task CreateCommandAsync(MdbContext mdb)
		{
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			{
				StringBuilder s = new StringBuilder();
				_schema = await f.CreateJMXRepo().GetSchemaAsync(_viewName);
				if (_schema.DbObjectType == DbObjectTypes.Procedure || 
					_schema.DbObjectType == DbObjectTypes.Action)
					_body = _schema.DbObjectName;
				else if (_schema.DbObjectType == DbObjectTypes.View ||
					_schema.DbObjectType == DbObjectTypes.Function ||
					_schema.DbObjectType == DbObjectTypes.Table)
				{
					//Костыль добавить IsNull, для Table Ref fields ...
					s.AppendLine("SELECT");
					var atrID = _schema.Attributes.FirstOrDefault(a => a.AttribName.ToUpper() == "ID");
					s.AppendLine($"\t{atrID.FieldName} {atrID.AttribName}");
					foreach (var att in _schema.Attributes.Where(a => a.AttribName.ToUpper() != "ID"))
						s.AppendLine($"\t,{att.FieldName} {att.AttribName}");
					s.AppendLine($"FROM {_schema.DbObjectName} AS {_schema.DbObjectName.ObjectName}");
					if (!_filter.IsEmpty())
						s.AppendLine($"WHERE ({_filter})");
					if (!_sort.IsEmpty())
						s.AppendLine($"ORDER BY ({_sort})");
					_body = s.ToString();
				}
			}
		}
	}
}
