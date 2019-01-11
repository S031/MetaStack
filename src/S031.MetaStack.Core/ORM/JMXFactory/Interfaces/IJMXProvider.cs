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
		JMXObject Read(JMXObjectName objectName, int id);
		JMXObject Read(JMXObjectName objectName, string alterKeyIndexName, params object[] parameters);
		void Delete(JMXObject jmxObject);
		void Delete(JMXObjectName objectName, int id);
		int Save(JMXObject jmxObject, bool isNew);
		int SetState(string objectName, int handle, int newState, IDictionary<string, object> paramList);
		Task<JMXObject> ReadAsync(JMXObjectName objectName, int id);
		Task<JMXObject> ReadAsync(JMXObjectName objectName, string alternameIndexName, params object[] parameters);
		Task DeleteAsync(JMXObject jmxObject);
		Task DeleteAsync(JMXObjectName objectName, int id);
		Task<int> SaveAsync(JMXObject jmxObject, bool isNew);
		Task<int> SetStateAsync(string objectName, int handle, int newState, IDictionary<string, object> paramList);
	}
}
