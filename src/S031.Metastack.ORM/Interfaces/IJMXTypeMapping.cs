using System.Collections.Generic;
using S031.MetaStack.Data;

namespace S031.MetaStack.ORM
{
	public interface IJMXTypeMapping
	{
		IEnumerable<string> GetVariableLenghtDataTypes();
		IReadOnlyDictionary<string, MdbTypeInfo> GetServerTypeMap();
		IReadOnlyDictionary<MdbType, string> GetTypeMap();
	}
}
