using Microsoft.Extensions.Logging;
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
	public abstract class JMXRepo : IJMXRepo, IDisposable
	{
		private ILogger _logger;
		private bool _isLocalLog;

		public JMXRepo(MdbContext mdbContext)
		{
			mdbContext.NullTest(nameof(mdbContext));
			this.MdbContext = mdbContext;
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
					throw new InvalidOperationException($"The Logger has already been assigned this instance of the {this.GetType().FullName} class");
				_logger = value;
				_isLocalLog = false;
			}
		}
		
		protected MdbContext MdbContext { get; private set; }

		public virtual IDictionary<MdbType, string> GetTypeMap()
		{
			throw new NotImplementedException();
		}

		public virtual IDictionary<string, MdbTypeInfo> GetServerTypeMap()
		{
			throw new NotImplementedException();
		}

		public virtual string[] GetVariableLenghtDataTypes()
		{
			throw new NotImplementedException();
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
	}
}
