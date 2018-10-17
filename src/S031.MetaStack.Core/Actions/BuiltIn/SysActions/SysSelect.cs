using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.Actions
{
	internal class SysSelect : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			List<object> parameters = new List<object>();
			string connectionName = "BankLocal";
			string viewName = string.Empty;
			for (; dp.Read();)
			{
				string paramName = (string)dp["ParamName"];
				if (paramName[0] == '@')
				{
					parameters.Add(dp["ParamName"]);
					parameters.Add(dp["ParamValue"]);
				}
				else if (paramName.Equals("_connectionName", StringComparison.CurrentCultureIgnoreCase))
					connectionName = (string)dp["ParamValue"];
				else if (paramName.Equals("_viewName", StringComparison.CurrentCultureIgnoreCase))
					viewName = (string)dp["ParamValue"];
			}
			if (viewName.IsEmpty())
				viewName = "Select Top 1 ID, UID, ObjectName From SysCat.SysSchemas";
			var configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			var cs = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			using (MdbContext mdb = new MdbContext(cs))
			{
				return mdb.GetReader(viewName, parameters.ToArray());
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			List<object> parameters = new List<object>();
			string connectionName = "BankLocal";
			string viewName = string.Empty;
			for (; dp.Read();)
			{
				string paramName = (string)dp["ParamName"];
				if (paramName[0] == '@')
				{
					parameters.Add(dp["ParamName"]);
					parameters.Add(dp["ParamValue"]);
				}
				else if (paramName.Equals("_connectionName", StringComparison.CurrentCultureIgnoreCase))
					connectionName = (string)dp["ParamValue"];
				else if (paramName.Equals("_viewName", StringComparison.CurrentCultureIgnoreCase))
					viewName = (string)dp["ParamValue"];
			}
			if (viewName.IsEmpty())
				viewName = "Select Top 1 ID, UID, ObjectName From SysCat.SysSchemas";
			var configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			var cs = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(cs))
			{
				return await mdb.GetReaderAsync(viewName, parameters.ToArray());
			}
		}
	}
}
