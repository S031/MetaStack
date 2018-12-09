using Microsoft.Extensions.Configuration;
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

			var ctx = ai.GetContext();
			var cs = ApplicationContext
				.GetConfiguration()
				.GetSection($"connectionStrings:{ctx.ConnectionName}").Get<ConnectInfo>();

			var pipe = ApplicationContext.GetPipe();
			StringBuilder sb = new StringBuilder();
			//foreach (long i in ids)
			//{
			using (MdbContext mdb = new MdbContext(cs))
			{
				try
				{
					await mdb.ExecuteAsync<string>($@"
					exec workdb..mib_msfo_dv_calc 
						@Date = '{date.ToString("yyyyMMdd")}'
						,@BranchID = {branchID}
						,@DealType = '{dealType}'
						,@ReCalc   = 1");
					string result = $"Success mib_msfo_dv_calc {date} {branchID} {dealType}";
					pipe.Write(ctx, result);
					sb.AppendLine(result);
				}
				catch (Exception ex)
				{
					sb.AppendLine($"{LogLevels.Error} {ex.Message}");
				}
			}
			//}
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", sb.ToString())
				.Update();
		}
	}
}

