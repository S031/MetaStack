﻿using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metib.Business.Msfo
{
	public class PDCalculate : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			DateTime date = (DateTime)dp["@Date"];
			string branchID = (string)dp["@BranchID"];
			string dealType = (string)dp["@DealType"];

			var ctx = ai.GetContext();
			var cs = ApplicationContext
				.GetConfiguration()
				.GetSection($"connectionStrings:{ctx.ConnectionName}").Get<ConnectInfo>();

			string[] filials = DealValuesCalculate.GetParamListData(ai, "@BranchID", branchID, "0");
			string[] dealTypes = DealValuesCalculate.GetParamListData(ai, "@DealType", dealType, "Все");

			var pipe = ApplicationContext.GetPipe();
			StringBuilder sb = new StringBuilder();
			foreach (var filial in filials)
			{
				foreach (var dtype in dealTypes)
				{
					using (MdbContext mdb = new MdbContext(cs))
					{
						try
						{
							await mdb.ExecuteAsync<string>($@"
							exec workdb..mib_msfo_pd_calc 
								@Date = '{date.ToString("yyyyMMdd")}'
								,@BranchID = {filial}
								,@DealType = '{dtype}'");
							string result = $"Success mib_msfo_pd_calc {date} filial={branchID} deal type={dealType}";
							pipe.Write(ctx, result);
							sb.AppendLine(result);
						}
						catch (Exception ex)
						{
							string message = $"{LogLevels.Error} {ex.Message}";
							pipe.Write(ctx, message);
							sb.AppendLine(message);
						}
					}
				}
			}
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", sb.ToString())
				.Update();
		}
	}

}

