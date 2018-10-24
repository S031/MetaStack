﻿using System;
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
				var dr = ai.GetOutputParamTable();
				dr.AddNew();
				dr["ObjectSchema"] = (f.CreateJMXRepo().SaveSchema(_schemaSource)).ToString();
				dr.Update();
				return dr;
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			using (JMXFactory f = JMXFactory.Create(mdb, ApplicationContext.GetLogger()))
			{
				var dr = ai.GetOutputParamTable();
				dr.AddNew();
				dr["ObjectSchema"] = (await f.CreateJMXRepo().SaveSchemaAsync(_schemaSource)).ToString();
				dr.Update();
				return dr;
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			var configuration = ApplicationContext.GetConfiguration();
			if (!dp.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = configuration["appSettings:defaultConnection"];
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

			if (dp.Read())
				_schemaSource = JMXSchema.Parse((string)dp["ObjectName"]);
		}
	}
}
