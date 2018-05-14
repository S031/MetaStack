using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.SysCat
{
	public partial class SysCatManager
	{
		public async Task SyncSchemaAsync(string objectName)
		{
			var t = await getSchemaAsync(objectName);
		}

	}
}
