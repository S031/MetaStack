using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metib.Business.Msfo
{
	public class MsfoAseTest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			string objectName = (string)dp["@ObjectName"];
			long[] ids = ((string)dp["@IDs"])
				.Split(new char[] { ',', ';' })
				.Select(s => s.ToLongOrDefault())
				.ToArray();

			var ctx = ai.GetContext();
			var cs = ApplicationContext
				.GetConfiguration()
				.GetSection($"connectionStrings:{ctx.ConnectionName}").Get<ConnectInfo>();

			var pipe = ApplicationContext.GetPipe();
			StringBuilder sb = new StringBuilder();
			foreach (long i in ids)
			{
				using (MdbContext mdb = new MdbContext(cs))
				{
					try
					{
						string result = await mdb.ExecuteAsync<string>($@"
							select 'Session ID = ' + cast(@@SPID as varchar(10))
							+ ' User Name = ' + '{ctx.UserName}'
							+ ' Connection Name = ' + '{ctx.ConnectionName}'
							+ ' Param Value = ' + '{i}'");

						pipe.Write(ctx, result);
						sb.AppendLine(result);
					}
					catch (Exception ex)
					{
						string message = $"{LogLevels.Error} {ex.Message}\n{ex.StackTrace}";
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
