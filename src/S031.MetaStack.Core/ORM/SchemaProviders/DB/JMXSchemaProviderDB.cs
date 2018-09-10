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
	public partial class JMXSchemaProviderDB : IJMXSchemaProvider, IDisposable
	{
		object objLock = new object();
		private ILogger _logger;
		private string _cn;
		private static string _defaultDbSchema = string.Empty;
		private ISchemaDBSync _syncronizer;
		private bool _isLocalLog;

		private static int _counter = 0;
		static readonly Dictionary<string, JMXSchema> _schemaCache = new Dictionary<string, JMXSchema>();
		static readonly Dictionary<string, List<string>> _parentRelations = new Dictionary<string, List<string>>();

		public JMXSchemaProviderDB(string connectionString)
			: this(connectionString, new Logging.FileLogger(typeof(JMXSchemaProviderDB).FullName))
		{
			_isLocalLog = true;
		}

		public JMXSchemaProviderDB(string connectionString, ILogger logger)
		{
			connectionString.NullTest(nameof(connectionString));
			logger.NullTest(nameof(logger));
			_logger = logger;
			_isLocalLog = false;
			_cn = connectionString;
			string dbProviderName = MdbContext.getConnectionInfo(connectionString).ProviderName;
			if (_counter == 0)
				ImplementsList.Add(typeof(ISchemaDBSync));

			_syncronizer = GetSyncronizer(dbProviderName);
			_syncronizer.SchemaProvider = this;
			_syncronizer.Logger = logger;
			if (_counter == 0)
			{
				using (var mdb = new MdbContext(_cn))
				{
					if (!_syncronizer.TestSysCatAsync(mdb).GetAwaiter().GetResult())
						_syncronizer.CreateDbSchemaAsync(mdb).GetAwaiter().GetResult();
					_defaultDbSchema = _syncronizer.GetDefaultDbSchemaAsync(mdb).GetAwaiter().GetResult();
					Interlocked.Increment(ref _counter);
				}
			}
		}

		private static ISchemaDBSync GetSyncronizer(string dbProviderName)
		{
			var l = ImplementsList.GetTypes(typeof(ISchemaDBSync));
			if (l == null)
				throw new InvalidOperationException("No class inherited from ISchemaDBSync defined");
			foreach (var t in l)
			{
				SchemaDBSyncAttribute att = (System.Attribute.GetCustomAttributes(t)?
					.FirstOrDefault(attr => attr.GetType() == typeof(SchemaDBSyncAttribute) &&
					(attr as SchemaDBSyncAttribute)?.DBProviderName == dbProviderName) as SchemaDBSyncAttribute);
				if (att != null)
					return (ISchemaDBSync)t.CreateInstance();
			}
			throw new InvalidOperationException("No class inherited from ISchemaDBSync contained attribute of type SchemaDBSyncAttribute  defined");
		}

		public  JMXSchema GetSchema(string objectName)
		{
			return GetSchemaAsync(objectName).GetAwaiter().GetResult();
		}

		public async Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			if (_schemaCache.TryGetValue(name, out JMXSchema schema))
			{
				schema.SchemaProvider = this;
				return schema;
			}
			using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
			{
				schema = await _syncronizer.GetSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
				lock (objLock)
				{
					if (!_schemaCache.ContainsKey(name))
						_schemaCache.Add(name, schema);
				}
			}
			return schema;
		}

		public JMXSchema SaveSchema(JMXSchema schema)
		{
			return SaveSchemaAsync(schema).GetAwaiter().GetResult();
		}

		public async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
			{
				await _syncronizer.SaveSchemaAsync(mdb, schema).ConfigureAwait(false);
			}
			lock (objLock)
				_schemaCache[schema.ObjectName] = schema;
			foreach (var fk in schema.ForeignKeys)
			{
				if (fk.RefObjectName.IsEmpty())
					throw new ArgumentNullException("Property RefObjectName can't be empty");
				lock (objLock)
				{
					if (_parentRelations.ContainsKey(fk.RefObjectName))
						_parentRelations[fk.RefObjectName].Add(schema.ObjectName);
					else
						_parentRelations.Add(fk.RefObjectName, new List<string>() { schema.ObjectName });
				}
			}
			return schema;
		}

		public  IEnumerable<string> GetChildObjects(string objectName)
		{
			if (_parentRelations.TryGetValue(objectName, out var childObjectList))
				return childObjectList;
			return new List<string>();
		}

		public async Task ClearCatalogAsync()
		{
			using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
			{
				await _syncronizer.DropDbSchemaAsync(mdb).ConfigureAwait(false);
			}
		}

		public async Task<JMXSchema> SyncSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
			{
				var schema = await _syncronizer.SyncSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
				lock (objLock)
					_schemaCache[schema.ObjectName] = schema;
				return schema;
			}
		}

		public void DropSchema(string objectName)
		{
			DropSchemaAsync(objectName).GetAwaiter().GetResult();
		}

		public async Task DropSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			using (var mdb = await MdbContext.CreateMdbContextAsync(_cn).ConfigureAwait(false))
			{
				await _syncronizer.DropSchemaAsync(mdb, name.AreaName, name.ObjectName).ConfigureAwait(false);
			}
			lock (objLock)
				_schemaCache.Remove(name.ToString());
		}

		public void Dispose()
		{
			if (_isLocalLog)
				(_logger as IDisposable)?.Dispose();
		}
	}
}
