using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common;
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


			StringBuilder sb = new StringBuilder();
			foreach (long i in ids)
			{
				using (MdbContext mdb = new MdbContext(cs))
				{
					try
					{
						sb.AppendLine(await mdb.ExecuteAsync<string>($"exec kalv_TestForFct {i}"));
					}
					catch (Exception ex)
					{
						return DataPackage.CreateErrorPackage(ex);
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
