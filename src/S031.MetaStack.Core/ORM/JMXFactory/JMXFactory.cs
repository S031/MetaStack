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
	public abstract class JMXFactory : IDisposable
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

		public JMXFactory(MdbContext mdbContext)
		{
			mdbContext.NullTest(nameof(mdbContext));
			this.MdbContext = mdbContext;
		}

		public MdbContext MdbContext { get; }

		public virtual IJMXRepo CreateJMXRepo()
		{
			throw new NotImplementedException();
		}
		public virtual IJMXProvider CreateJMXProvider()
		{
			throw new NotImplementedException();
		}
		public virtual JMXObject CreateObject(string objectName)
		{
			throw new NotImplementedException();
		}

		public static JMXFactory Create(MdbContext mdb, ILogger logger)
		{
			var l = ImplementsList.GetTypes(typeof(JMXFactory));
			if (l == null)
				throw new InvalidOperationException("No class inherited from JMXFactory defined");
			string dbProviderName = mdb.ProviderName.ToLower();
			foreach (var t in l)
			{
				SchemaDBSyncAttribute att = (System.Attribute.GetCustomAttributes(t)?
					.FirstOrDefault(attr => attr.GetType() == typeof(SchemaDBSyncAttribute) &&
					(attr as SchemaDBSyncAttribute)?.DBProviderName.ToLower() == dbProviderName) as SchemaDBSyncAttribute);
				if (att != null)
					return (JMXFactory)t.CreateInstance(mdb, logger);
			}
			throw new InvalidOperationException("No class inherited from ISchemaDBSync contained attribute of type SchemaDBSyncAttribute  defined");
		}
	}
}
