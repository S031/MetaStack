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
	public abstract class JMXRepo : IJMXSRepo, IDisposable
	{
		private ILogger _logger;
		private bool _isLocalLog;

		//public JMXRepo(ConnectInfo connectInfo)
		//{ }

		//public JMXRepo(string connectionString)
		//	: this(connectionString, new Logging.FileLogger(typeof(JMXSchemaProviderDB).FullName))
		//{
		//	_isLocalLog = true;
		//}

		//public JMXRepo(string connectionString, ILogger logger)
		//{
		//	connectionString.NullTest(nameof(connectionString));
		//	logger.NullTest(nameof(logger));
		//	_logger = logger;
		//	_isLocalLog = false;
		//	_cn = connectionString;
		//	string dbProviderName = MdbContext.getConnectionInfo(connectionString).ProviderName;
		//	if (_counter == 0)
		//		ImplementsList.Add(typeof(ISchemaDBSync));

		//	_syncronizer = GetSyncronizer(dbProviderName);
		//	_syncronizer.SchemaProvider = this;
		//	_syncronizer.Logger = logger;
		//	if (_counter == 0)
		//	{
		//		using (var mdb = new MdbContext(_cn))
		//		{
		//			if (!_syncronizer.TestSysCatAsync(mdb).GetAwaiter().GetResult())
		//				_syncronizer.CreateDbSchemaAsync(mdb).GetAwaiter().GetResult();
		//			_defaultDbSchema = _syncronizer.GetDefaultDbSchemaAsync(mdb).GetAwaiter().GetResult();
		//			Interlocked.Increment(ref _counter);
		//		}
		//	}
		//}



		//private static ISchemaDBSync GetSyncronizer(string dbProviderName)
		//{
		//	var l = ImplementsList.GetTypes(typeof(ISchemaDBSync));
		//	if (l == null)
		//		throw new InvalidOperationException("No class inherited from ISchemaDBSync defined");
		//	foreach (var t in l)
		//	{
		//		SchemaDBSyncAttribute att = (System.Attribute.GetCustomAttributes(t)?
		//			.FirstOrDefault(attr => attr.GetType() == typeof(SchemaDBSyncAttribute) &&
		//			(attr as SchemaDBSyncAttribute)?.DBProviderName == dbProviderName) as SchemaDBSyncAttribute);
		//		if (att != null)
		//			return (ISchemaDBSync)t.CreateInstance();
		//	}
		//	throw new InvalidOperationException("No class inherited from ISchemaDBSync contained attribute of type SchemaDBSyncAttribute  defined");
		//}

		//public JMXSchema GetSchema(string objectName)
		//{
		//	return GetSchemaAsync(objectName).GetAwaiter().GetResult();
		//}

		//public async Task<JMXSchema> GetSchemaAsync(string objectName)
		//{
		//	JMXObjectName name = objectName;
		//	if (_schemaCache.TryGetValue(name, out JMXSchema schema))
		//	{
		//		schema.SchemaProvider = this;
		//		return schema;
		//	}
		//	using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
		//	{
		//		schema = await _syncronizer.GetSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
		//		lock (obj4Lock)
		//		{
		//			if (!_schemaCache.ContainsKey(name))
		//				_schemaCache.Add(name, schema);
		//		}
		//	}
		//	return schema;
		//}

		//public JMXSchema SaveSchema(JMXSchema schema)
		//{
		//	return SaveSchemaAsync(schema).GetAwaiter().GetResult();
		//}

		//public async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		//{
		//	using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
		//	{
		//		await _syncronizer.SaveSchemaAsync(mdb, schema).ConfigureAwait(false);
		//	}
		//	lock (obj4Lock)
		//		_schemaCache[schema.ObjectName] = schema;
		//	foreach (var fk in schema.ForeignKeys)
		//	{
		//		if (fk.RefObjectName.IsEmpty())
		//			throw new ArgumentNullException("Property RefObjectName can't be empty");
		//		lock (obj4Lock)
		//		{
		//			if (_parentRelations.ContainsKey(fk.RefObjectName))
		//				_parentRelations[fk.RefObjectName].Add(schema.ObjectName);
		//			else
		//				_parentRelations.Add(fk.RefObjectName, new List<string>() { schema.ObjectName });
		//		}
		//	}
		//	return schema;
		//}

		//public IEnumerable<string> GetChildObjects(string objectName)
		//{
		//	if (_parentRelations.TryGetValue(objectName, out var childObjectList))
		//		return childObjectList;
		//	return new List<string>();
		//}

		//public async Task ClearCatalogAsync()
		//{
		//	using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
		//	{
		//		await _syncronizer.DropDbSchemaAsync(mdb).ConfigureAwait(false);
		//	}
		//}

		//public async Task<JMXSchema> SyncSchemaAsync(string objectName)
		//{
		//	JMXObjectName name = objectName;
		//	using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
		//	{
		//		var schema = await _syncronizer.SyncSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
		//		lock (obj4Lock)
		//			_schemaCache[schema.ObjectName] = schema;
		//		return schema;
		//	}
		//}

		//public void DropSchema(string objectName)
		//{
		//	DropSchemaAsync(objectName).GetAwaiter().GetResult();
		//}

		//public async Task DropSchemaAsync(string objectName)
		//{
		//	JMXObjectName name = objectName;
		//	using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
		//	{
		//		await _syncronizer.DropSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
		//	}
		//	lock (obj4Lock)
		//		_schemaCache.Remove(name.ToString());
		//}

		public JMXRepo(MdbContext mdbContext)
		{
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
