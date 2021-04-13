using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
			var cs = ctx.Services
				.GetRequiredService<IConfiguration>()
				.GetSection($"connectionStrings:{ctx.ConnectionName}").Get<ConnectInfo>();

			string[] filials = DealValuesCalculate.GetParamListData(ai, "@BranchID", branchID, "0");
			string[] dealTypes = DealValuesCalculate.GetParamListData(ai, "@DealType", dealType, "Все");

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
							exec workdb..mib_msfo_pd_calc 
								@Date = '{date.ToString("yyyyMMdd")}'
								,@BranchID = {filial}
								,@DealType = '{dtype}'");
							string result = $"Success mib_msfo_pd_calc {date} filial={filial} deal type={dtype}";
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
	}

}

