using Microsoft.Extensions.Logging;
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
					_logger = new Logging.FileLogger(typeof(JMXRepo).FullName);
					_isLocalLog = true;
				}
				return _logger;
			}
			set
			{
				if (_logger != null)
					throw new InvalidOperationException("The Logger has already been assigned this instance of the JMXRepo class");
				_logger = value;
				_isLocalLog = false;
			}
		}

		protected MdbContext MdbContext { get; private set; }

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
