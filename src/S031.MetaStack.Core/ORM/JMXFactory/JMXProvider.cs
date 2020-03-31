using S031.MetaStack.Common;
using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public class JMXProvider : ManagerObjectBase, IJMXProvider, IDisposable
	{
		protected readonly static MapTable<string, string> _statementsCache = new MapTable<string, string>();
		private readonly JMXFactory _factory;
		public JMXProvider(JMXFactory factory) : 
			base(factory.GetMdbContext(ContextTypes.SysCat), factory.GetMdbContext(ContextTypes.Work))
		{
			_factory = factory;
			base.Logger = factory.Logger;
		}
		protected virtual JMXFactory Factory => _factory;

		public virtual void Delete(JMXObject jmxObject) 
			=> Delete(jmxObject.ObjectName, jmxObject.ID);

		public virtual void Delete(JMXObjectName objectName, long id)
			=> DeleteAsync(objectName, id).GetAwaiter().GetResult();

		public virtual Task DeleteAsync(JMXObject jmxObject)
		{
			throw new NotImplementedException();
		}

		public async virtual Task DeleteAsync(JMXObjectName objectName, long id)
		{
			JMXObject jo = await ReadAsync(objectName, id);
			if (jo == null)
				throw new InvalidOperationException(Properties.Strings.S031_MetaStack_Core_ORM_JMXProvider_ObjectNotFound
					.ToFormat(objectName, id));
			await DeleteAsync(jo);
		}

		public virtual JMXObject Read(JMXObjectName objectName, long id)
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject Read(JMXObjectName objectName, string alterKeyIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXObject> ReadAsync(JMXObjectName objectName, long id)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXObject> ReadAsync(JMXObjectName objectName, string alterKeyIndexName, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public virtual long Save(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public virtual Task<long> SaveAsync(JMXObject jmxObject, bool isNew)
		{
			throw new NotImplementedException();
		}

		public virtual int SetState(string objectName, long handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}

		public virtual Task<int> SetStateAsync(string objectName, long handle, int newState, IDictionary<string, object> paramList)
		{
			throw new NotImplementedException();
		}
	}
}
