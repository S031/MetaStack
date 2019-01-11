using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public class JMXProvider : ManagerObjectBase, IJMXProvider, IDisposable
	{
		JMXFactory _factory;
		public JMXProvider(JMXFactory factory) : 
			base(factory.GetMdbContext(ContextTypes.SysCat), factory.GetMdbContext(ContextTypes.Work))
		{
			_factory = factory;
			base.Logger = factory.Logger;
		}

		public virtual void Delete(JMXObject jmxObject) 
			=> Delete(jmxObject.ObjectName, jmxObject.ID);

		public virtual void Delete(JMXObjectName objectName, int id)
			=> DeleteAsync(objectName, id).GetAwaiter().GetResult();

		public virtual async Task DeleteAsync(JMXObject jmxObject)
			=> await DeleteAsync(jmxObject.ObjectName, jmxObject.ID);

		public virtual Task DeleteAsync(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject Read(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject Read(JMXObjectName objectName, string alterKeyIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXObject> ReadAsync(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXObject> ReadAsync(JMXObjectName objectName, string alternameIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public virtual int Save(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public virtual Task<int> SaveAsync(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public virtual int SetState(string objectName, int handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}

		public virtual Task<int> SetStateAsync(string objectName, int handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}
	}
}
