using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Integral.Settings
{
	public interface ISettingsProvider<T> where T : class, ISettingsProvider<T>
	{
		Task<JsonValue> GetSettings(string path);
	}
}
