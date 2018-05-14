using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public abstract class JMXSchemaProviderBase : IJMXSchemaProvider
	{
		readonly object objLock = new object();

		static readonly Dictionary<string, JMXSchema> _schemaCache = new Dictionary<string, JMXSchema>();
		static readonly Dictionary<string, List<string>> _parentRelations = new Dictionary<string, List<string>>();

		public virtual JMXSchema GetSchema(string objectName)
		{
			if (_schemaCache.TryGetValue(objectName, out JMXSchema schema))
			{
				schema.SchemaProvider = this;
				return schema;
			}
			return null;
		}

		public virtual async Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			return await Task<JMXSchema>.Run(() => GetSchema(objectName)).ConfigureAwait(false);
		}

		public virtual JMXSchema SaveSchema(JMXSchema schema)
		{
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

		public void DropSchema(string objectName)
		{
			lock (objLock)
				_schemaCache.Remove(objectName);

		}
		public virtual IEnumerable<string> GetChildObjects(string objectName)
		{
			if (_parentRelations.TryGetValue(objectName, out var childObjectList))
				return childObjectList;
			return new List<string>();
		}

		public virtual async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			return await Task<JMXSchema>.Run(() => SaveSchema(schema)).ConfigureAwait(false);
		}

		public Task<JMXSchema> SyncSchemaAsync(string ObjectName)
		{
			throw new NotImplementedException();
		}

		public virtual async Task DropSchemaAsync(string objectName)
		{
			await Task.Run(() => DropSchema(objectName)).ConfigureAwait(false);
		}
	}
	public class JMXSchemaProviderMemory : JMXSchemaProviderBase
	{
		public static JMXSchemaProviderMemory Instance = new JMXSchemaProviderMemory();
	}
}
