﻿using System;
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
		readonly List<object> _parameters = new List<object>();
		ConnectInfo _connectInfo;
		string _viewName;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			{
				return mdb.GetReader(_viewName, _parameters.ToArray());
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			{
				return await mdb.GetReaderAsync(_viewName, _parameters.ToArray());
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
			}
			if (_viewName.IsEmpty())
				_viewName = "Select Top 1 * From SysCat.SysSchemas";
			var configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

		}
	}
}
