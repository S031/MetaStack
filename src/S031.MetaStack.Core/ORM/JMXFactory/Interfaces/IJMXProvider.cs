using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXProvider
    {
		JMXObject Read(JMXObjectName objectName, long id);
		JMXObject Read(JMXObjectName objectName, string alterKeyIndexName, params object[] parameters);
		void Delete(JMXObject jmxObject);
		void Delete(JMXObjectName objectName, long id);
		long Save(JMXObject jmxObject, bool isNew);
		int SetState(string objectName, long handle, int newState, IDictionary<string, object> paramList);
		Task<JMXObject> ReadAsync(JMXObjectName objectName, long id);
		Task<JMXObject> ReadAsync(JMXObjectName objectName, string alternameIndexName, params object[] parameters);
		Task DeleteAsync(JMXObject jmxObject);
		Task DeleteAsync(JMXObjectName objectName, long id);
		Task<long> SaveAsync(JMXObject jmxObject, bool isNew);
		Task<int> SetStateAsync(string objectName, long handle, int newState, IDictionary<string, object> paramList);
	}
}
