﻿using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Metib.Business.Msfo
{
	public class DealValuesCalculate : IAppEvaluator
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
			int reCalc = (int)dp["@ReCalc"];

			var ctx = ai.GetContext();
			var cs = ctx.Services
				.GetRequiredService<IConfiguration>()
				.GetSection($"connectionStrings:{ctx.ConnectionName}").Get<ConnectInfo>();

			string[] filials = GetParamListData(ai, "@BranchID", branchID, "0");
			string[] dealTypes = GetParamListData(ai, "@DealType", dealType, "Все");

			var pipe = ctx.Services
				.GetRequiredService<PipeQueue>();
			StringBuilder sb = new StringBuilder();
			foreach (var filial in filials)
			{
				foreach (var dtype in dealTypes)
				{
					using (MdbContext mdb = new MdbContext(cs))
					{
						try
						{
							await mdb.ExecuteAsync($@"
							exec workdb..mib_msfo_dv_calc 
								@Date = '{date:yyyyMMdd}'
								,@BranchID = {filial}
								,@DealType = '{dtype}'
								,@ReCalc   = {reCalc}");
							string result = $"Success mib_msfo_dv_calc {date} filial={filial} deal type={dtype}";
							pipe.Write(ctx, result);
							sb.AppendLine(result);
						}
						catch (Exception ex)
						{
							string message = $"{LogLevels.Error} {ex.Message}\n{ex.StackTrace}";
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

		internal static string[] GetParamListData(ActionInfo ai, string paramName, string paramValue, string allValueKey)
		{
			return !paramValue.Equals(allValueKey, StringComparison.OrdinalIgnoreCase)
				? new string[] { paramValue }
				: ai.InterfaceParameters.FirstOrDefault(p => p.Value.ParameterID == paramName)
					.Value
					.ListData
					.Split(',')
					.Where(s => s != allValueKey)
					.ToArray();
		}
	}

}

