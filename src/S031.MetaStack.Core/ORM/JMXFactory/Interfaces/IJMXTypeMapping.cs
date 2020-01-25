using System.Collections.Generic;
using S031.MetaStack.Data;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXTypeMapping
	{
		IEnumerable<string> GetVariableLenghtDataTypes();
		IReadOnlyDictionary<string, MdbTypeInfo> GetServerTypeMap();
		IReadOnlyDictionary<MdbType, string> GetTypeMap();
	}
}
