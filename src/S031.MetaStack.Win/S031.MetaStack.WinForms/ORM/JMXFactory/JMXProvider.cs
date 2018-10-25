using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public abstract class JMXProvider : IJMXProvider
	{
		public void Delete(JMXObject jmxObject)
		{
			throw new NotImplementedException();
		}

		public void Delete(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(JMXObject jmxObject)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public JMXObject Read(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public JMXObject Read(JMXObjectName objectName, string alternameIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public Task<JMXObject> ReadAsync(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public Task<JMXObject> ReadAsync(JMXObjectName objectName, string alternameIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public int Save(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public Task<int> SaveAsync(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public int SetState(string objectName, int handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}

		public Task<int> SetStateAsync(string objectName, int handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}
	}
}
