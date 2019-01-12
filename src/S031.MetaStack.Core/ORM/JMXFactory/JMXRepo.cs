﻿using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXRepo : ManagerObjectBase, IJMXRepo, IDisposable
	{
		private readonly JMXFactory _factory;
		public JMXRepo(JMXFactory factory) :
			base(factory.GetMdbContext(ContextTypes.SysCat), factory.GetMdbContext(ContextTypes.Work))
		{
			_factory = factory;
			base.Logger = factory.Logger;
		}

		public virtual IEnumerable<string> GetChildObjects(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual JMXSchema GetSchema(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual JMXSchema SaveSchema(JMXSchema schema)
		{
			throw new NotImplementedException();
		}

		public virtual void DropSchema(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual void ClearCatalog()
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> SyncSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual Task DropSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual Task ClearCatalogAsync()
		{
			throw new NotImplementedException();
		}
	}
}
