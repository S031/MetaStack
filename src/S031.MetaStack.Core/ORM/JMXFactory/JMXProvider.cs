﻿using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public class JMXProvider : IJMXProvider, IDisposable
	{
		private ILogger _logger;
		private bool _isLocalLog;

		public JMXProvider(MdbContext mdbContext)
		{
			mdbContext.NullTest(nameof(mdbContext));
			MdbContext = mdbContext;
		}

		public void Dispose()
		{
			if (_isLocalLog)
				(_logger as IDisposable)?.Dispose();
		}

		public ILogger Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = new Logging.FileLogger(this.GetType().FullName);
					_isLocalLog = true;
				}
				return _logger;
			}
			set
			{
				if (_logger != null)
					throw new InvalidOperationException($"The Logger has already been assigned this instance of the {this.GetType().FullName} class"); ;
				_logger = value;
				_isLocalLog = false;
			}
		}

		protected MdbContext MdbContext { get; private set; }

		public virtual void Delete(JMXObject jmxObject)
		{
			throw new NotImplementedException();
		}

		public virtual void Delete(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(JMXObject jmxObject)
		{
			throw new NotImplementedException();
		}

		public virtual Task DeleteAsync(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject Read(JMXObjectName objectName, int id)
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject Read(JMXObjectName objectName, string alternameIndexName, params object[] parameters)
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
