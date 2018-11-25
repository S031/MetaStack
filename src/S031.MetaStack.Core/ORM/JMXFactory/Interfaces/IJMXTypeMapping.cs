using System;
using System.Collections.Generic;
using System.Text;
#if NETCOREAPP
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Data;

namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXTypeMapping
	{
		IEnumerable<string> GetVariableLenghtDataTypes();
		IDictionary<string, MdbTypeInfo> GetServerTypeMap();
		IDictionary<MdbType, string> GetTypeMap();
	}
}
