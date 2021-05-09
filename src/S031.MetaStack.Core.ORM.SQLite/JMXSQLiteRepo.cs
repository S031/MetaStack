using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteRepo : JMXRepo
	{
		readonly object objLock = new object();
		static int _counter = 0;
		static readonly Dictionary<string, JMXSchema> _schemaCache = new Dictionary<string, JMXSchema>();
		static readonly Dictionary<string, List<string>> _parentRelations = new Dictionary<string, List<string>>();

		private enum AttribCompareDiff
		{
			none = 0,
			notFound = 2,
			name = 4,
			dataTtype = 8,
			size = 16,
			nullOptions = 32,
			remove = 64,
			constraint = 128,
			identity = 256
		}
		private enum DbObjectOnDiffActions
		{
			none = 0,
			add = 2,
			drop = 4,
			alter = 8,
		}
		const string schema_version = "0.0.1";

		private static readonly JMXSQLiteTypeMapping _typeMapping = new JMXSQLiteTypeMapping();
		private static IReadOnlyDictionary<MdbType, string> TypeMap => _typeMapping.GetTypeMap();
		private static IReadOnlyDictionary<string, MdbTypeInfo> TypeInfo => _typeMapping.GetServerTypeMap();
		private static string _defaultDbSchema = string.Empty;

		public ILogger Logger => Factory.Logger;

		public JMXSQLiteRepo(JMXSQLiteFactory factory) : base(factory)
		{
			TestSysCat();
		}

		public override JMXSchema GetSchema(string objectName) => GetSchemaAsync(objectName).GetAwaiter().GetResult();

		public override async Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			if (_schemaCache.TryGetValue(name, out JMXSchema schema))
			{
				schema.SchemaRepo = this;
				return schema;
			}
			schema = await GetSchemaAsync(Factory.GetMdbContext(), name.AreaName, name.ObjectName).ConfigureAwait(false);
			lock (objLock)
			{
				if (!_schemaCache.ContainsKey(name))
					_schemaCache.Add(name, schema);
			}
			return schema;
		}

		private async Task TestSysCatAsync()
		{
			var mdb = Factory.GetMdbContext();
			var result = await mdb.ExecuteAsync<string>(SQLite.TestSchema);
			if (result.IsEmpty())
			{
				await CreateDbSchemaAsync(mdb, Logger);
				_defaultDbSchema = await GetDefaultDbSchemaAsync(mdb);
			}
		}

		private void TestSysCat()
		{
			if (_counter == 0)
			{
				TestSysCatAsync()
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();
				Interlocked.Increment(ref _counter);
			}
		}

		private static async Task<string> GetDefaultDbSchemaAsync(MdbContext mdb)
		{
			if (_defaultDbSchema.IsEmpty())
				_defaultDbSchema = await mdb.ExecuteAsync<string>(SQLite.GetDefaultSchema);
			return _defaultDbSchema;
		}

		private static async Task CreateDbSchemaAsync(MdbContext mdb, ILogger log)
		{
			try
			{
				await mdb.BeginTransactionAsync();
				var scripts = SQLite.CreateSchemaObjects_12.Split(new string[] { "--go", "--GO" },
					StringSplitOptions.RemoveEmptyEntries);

				foreach (string statement in scripts)
					await mdb.ExecuteAsync(statement);
				log.LogDebug($"Schema SysCat was created in database {mdb.DbName}");
				log.LogDebug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
				string owner = await GetDefaultDbSchemaAsync(mdb);
				long id = await mdb.ExecuteAsync<long>(SQLite.AddSysAreas,
					new MdbParameter("@SchemaName", owner),
					new MdbParameter("@SchemaOwner", owner),
					new MdbParameter("@SchemaVersion", schema_version),
					new MdbParameter("@SchemaConfig", "") { NullIfEmpty = true },
					new MdbParameter("@IsDefault", 1),
					new MdbParameter("@UpdateTime", DateTime.Now),
					new MdbParameter("@DateBegin", DateTime.Now.Date),
					new MdbParameter("@DateEnd", DateTime.MinValue) { NullIfEmpty = true });
				id = await mdb.ExecuteAsync<long>(SQLite.AddSysAreas,
					new MdbParameter("@SchemaName", "SysCat"),
					new MdbParameter("@SchemaOwner", owner),
					new MdbParameter("@SchemaVersion", schema_version),
					new MdbParameter("@SchemaConfig", "") { NullIfEmpty = true },
					new MdbParameter("@IsDefault", 1),
					new MdbParameter("@UpdateTime", DateTime.Now),
					new MdbParameter("@DateBegin", DateTime.Now.Date),
					new MdbParameter("@DateEnd", DateTime.MinValue) { NullIfEmpty = true });
				await mdb.CommitAsync();
			}
			catch (Exception e)
			{
				mdb.RollBack();
				log.LogError($"CreateSchema error: {e.Message}");
				throw;
			}
		}

		private static async Task<JMXSchema> GetSchemaAsync(MdbContext mdb, string areaName, string objectName)
			=> await GetSchemaInternalAsync(mdb, areaName, objectName, 0).ConfigureAwait(false);

		private static async Task<JMXSchema> GetSchemaInternalAsync(MdbContext mdb, string areaName, string objectName, int syncState)
		{
			string sortDirrect = syncState == 0 ? "" : "desc";
			string sql = $@"
				select 
					ID
					,ObjectSchema
					,SyncState 
				from V_SysSchemas 
				where SchemaName = '{areaName}' 
					and (ObjectName = '{objectName}' or DbObjectName = '{objectName}') 
				and SyncState >= {syncState}
				order by SyncState {sortDirrect} limit 1";
			using (var dr = await mdb.GetReaderAsync(sql))
			{
				if (!dr.Read())
					//object schema not found in database
					throw new InvalidOperationException(string.Format(Properties.Strings.S031_MetaStack_Core_SysCat_SysCatManager_getSchema_2,
						$"{areaName}.{objectName}", syncState));
				var schema = JMXSchema.Parse((string)dr["ObjectSchema"]);
				schema.ID = (int)dr["ID"];
				schema.SyncState = (int)dr["SyncState"];
				return schema;
			}
		}

		#region Delete Schema
		public override void DeleteSchema(string objectName)
			=> DeleteSchemaAsync(objectName).GetAwaiter().GetResult();

		public override async Task DeleteSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			MdbContext mdb = Factory.GetMdbContext();
			var schema = await GetSchemaAsync(mdb, name.AreaName, name.ObjectName);

			await mdb.ExecuteAsync(SQLite.DelSysSchemas,
				new MdbParameter("@ID", schema.ID));
			_schemaCache.Remove(name.ToString());
		}

		#endregion Delete Schema

		#region Clear Catalog
		public override void ClearCatalog()
		{
			DropDbSchemaAsync(Factory.GetMdbContext(), Logger)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
		public override async Task ClearCatalogAsync()
		{
			await DropDbSchemaAsync(Factory.GetMdbContext(), Logger).ConfigureAwait(false);
		}
		private static async Task DropDbSchemaAsync(MdbContext mdb, ILogger log)
		{
			var test = await mdb.ExecuteAsync<string>(SQLite.TestSchema);
			if (!test.IsEmpty())
			{
				try
				{
					await mdb.ExecuteAsync(SQLite.DropSchema);
					log.LogDebug($"Schema SysCat was deleted from database {mdb.DbName}");
				}
				catch (Exception e)
				{
					log.LogError($"Delete Schema error: {e.Message}");
					throw;
				}
			}
			else
				log.LogDebug($"Schema SysCat not exists in database  {mdb.DbName}");
		}
		#endregion Clear Catalog

		#region Save Schema
		public override JMXSchema SaveSchema(JMXSchema schema) => SaveSchemaAsync(schema).GetAwaiter().GetResult();

		public override async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			var mdb = Factory.GetMdbContext();
			schema = await NormalizeSchemaAsync(mdb, schema);
			long id = await mdb.ExecuteAsync<long>(SQLite.AddSysSchemas,
					new MdbParameter("@uid", schema.UID),
					new MdbParameter("@SysAreaID",
						await mdb.ExecuteAsync<long>($"Select ID From SysAreas Where SchemaName = '{schema.ObjectName.AreaName}'")),
					new MdbParameter("@ObjectType", (int)schema.DbObjectType),
					new MdbParameter("@ObjectName", schema.ObjectName.ObjectName),
					new MdbParameter("@DbObjectName", schema.DbObjectName.ObjectName),
					new MdbParameter("@ObjectSchema", schema.ToString()),
					new MdbParameter("@Version", schema_version),
					new MdbParameter("@UpdateTime", DateTime.Now),
					new MdbParameter("@DateBegin", DateTime.Now.Date),
					new MdbParameter("@DateEnd", DateTime.MinValue) { NullIfEmpty = true },
					new MdbParameter("@PreviosID", 0) { NullIfEmpty = true });

			schema.ID = Convert.ToInt32(id);
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
		private static async Task<JMXSchema> NormalizeSchemaAsync(MdbContext mdb, JMXSchema schema)
		{
			NormalizeAttributes(schema);
			NormalizePK(schema);
			NormalizeIndexes(schema);
			await NormalizeFKAsync(mdb, schema);
			await NormalizeDetailsAsync(mdb, schema);
			return schema;
		}

		private static void NormalizeAttributes(JMXSchema schema)
		{
			if (schema.Attributes.Count == 0)
				//One or more attribute is required in the schema
				throw new InvalidOperationException(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_1);

			int i = 0;
			foreach (var att in schema.Attributes)
			{
				att.Position = ++i;
				att.IsPK = false;
				if (att.FieldName.IsEmpty())
					att.FieldName = att.AttribName;
				if (att.Identity.IsIdentity)
					att.IsNullable = false;

				if (att.DataType == MdbType.@null)
					//For an object type attribute, you must specify a schema
					throw new InvalidOperationException(string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_4,
						att.FieldName));

				string typeMap = TypeMap[att.DataType];
				if (att.ServerDataType.IsEmpty() || !typeMap.Contains(att.ServerDataType, StringComparison.OrdinalIgnoreCase))
					att.ServerDataType = typeMap.GetToken(0, ";");

				if (att.DataSize.IsEmpty())
				{
					MdbTypeInfo ti2 = TypeInfo[att.ServerDataType];
					if (!ti2.FixedSize)
						att.DataSize = new JMXDataSize(ti2.Size, ti2.Scale, ti2.Precision);
				}

				var cc = att.CheckConstraint;
				if (!cc.IsEmpty() && cc.ConstraintName.IsEmpty())
					cc.ConstraintName = $"CKC_{ att.FieldName}_{ schema.DbObjectName.ObjectName}";
				var dc = att.DefaultConstraint;
				if (!dc.IsEmpty() && dc.ConstraintName.IsEmpty())
					dc.ConstraintName = $"DF_{ att.FieldName}_{ schema.DbObjectName.ObjectName}";
			}
		}

		private static void NormalizePK(JMXSchema schema)
		{
			if (schema.PrimaryKey != null)
			{
				var pk = schema.PrimaryKey;
				if (pk.KeyName.IsEmpty())
					pk.KeyName = $"PK_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}";
				foreach (var member in pk.KeyMembers)
				{
					var att = schema.Attributes.FirstOrDefault(a => a.FieldName == member.FieldName);
					if (att != null)
					{
						att.IsPK = true;
						att.IsNullable = false;
					}
					else
						//The FieldName specified in the primary key is not in the attribute list
						throw new InvalidOperationException(
							string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3,
							member.FieldName, "primary key"));
				}
			}
			else if (schema.Attributes.Any(a => a.DataType == MdbType.@object))
			{
                //Requires PK for table with object attributes
                var pk = new JMXPrimaryKey(keyName: $"PK_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}");
				var att = schema.Attributes.FirstOrDefault(a => a.Identity.IsIdentity);
				if (att == null)
					att = schema.Attributes.FirstOrDefault(a => a.AttribName == "ID" || a.FieldName == "ID");
				if (att == null)
					att = schema.Attributes.FirstOrDefault(a => a.DefaultConstraint != null &&
						a.DefaultConstraint.Definition.IndexOf("next value for", StringComparison.OrdinalIgnoreCase) > -1);
				if (att != null)
				{
					if (att.FieldName.IsEmpty())
						att.FieldName = att.AttribName;
					pk.AddKeyMember(att.FieldName);
					schema.PrimaryKey = pk;
				}
				else
					//A primary key is required for a table that includes columns of type 'object'
					throw new InvalidOperationException(
						string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_5,
						schema.ObjectName));
			}
		}

		private static void NormalizeIndexes(JMXSchema schema)
		{
			int i = 0;
			foreach (var index in schema.Indexes)
			{
				if (index.IndexName.IsEmpty() && index.IsUnique)
					index.IndexName = $"AK{++i}_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}";
				else if (index.IndexName.IsEmpty())
					index.IndexName = $"IE{++i}_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}";
				foreach (var member in index.KeyMembers)
				{
					var att = schema.Attributes.FirstOrDefault(a => a.FieldName == member.FieldName);
					if (att == null)
						//The FieldName specified in the index is not in the attribute list
						throw new InvalidOperationException(
							string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3,
							member.FieldName, $"index '{index.IndexName}'"));
					else if (index.IsUnique)
						att.Required = true;
				}
			}
		}

		private static async Task NormalizeFKAsync(MdbContext mdb, JMXSchema schema)
		{
			//check indexes for FK!!!!
			foreach (var fk in schema.ForeignKeys)
			{
				if (fk.KeyName.IsEmpty())
					fk.KeyName = $"FK_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}_{fk.RefDbObjectName.ObjectName}";
				if (fk.RefObjectName.IsEmpty())
				{
					JMXSchema refSchema = await GetSchemaAsync(mdb, fk.RefDbObjectName.AreaName, fk.RefDbObjectName.ObjectName);
					if (refSchema == null)
						throw new ArgumentNullException("Property RefObjectName can't be empty");
					// set value, not ref
					fk.RefObjectName = refSchema.ObjectName.ToString();
				}

				foreach (var member in fk.KeyMembers)
				{
					var att = schema.Attributes.FirstOrDefault(a => a.FieldName == member.FieldName);
					if (att == null)
						//The FieldName specified in the foreign key is not in the attribute list
						throw new InvalidOperationException(
							string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3,
							$"foreign key '{fk.KeyName}'"));
				}
			}
		}

		private static async Task NormalizeDetailsAsync(MdbContext mdb, JMXSchema schema)
		{
			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
			{
				if (att.ObjectSchema == null)
					//For an object type attribute, you must specify a schema
					throw new InvalidOperationException(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_2);

				att.ObjectSchema = await NormalizeSchemaAsync(mdb, att.ObjectSchema);
				att.ServerDataType = "varchar";
				att.DataSize = new JMXDataSize(1);
				att.FieldName = $"{JMXFactory.DETAIL_FIELD_PREFIX}{att.ObjectSchema.DbObjectName.AreaName}_{att.ObjectSchema.DbObjectName.ObjectName}";
				//check indexes fo FK
				if (!att.ObjectSchema.ForeignKeys.Any(fk => fk.RefDbObjectName.AreaName == schema.DbObjectName.AreaName &&
					fk.RefDbObjectName.ObjectName == schema.DbObjectName.ObjectName))
				{
					var fk = new JMXForeignKey($"FK_{att.ObjectSchema.DbObjectName.AreaName}_{att.ObjectSchema.DbObjectName.ObjectName}_{schema.DbObjectName.ObjectName}")
					{
						RefDbObjectName = schema.DbObjectName.ToString(),
						RefObjectName = schema.ObjectName.ToString()
					};

					foreach (var m in schema.PrimaryKey.KeyMembers)
					{
						string fieldName = $"{schema.ObjectName}{m.FieldName}";
						if (!att.ObjectSchema.Attributes.Any(a => a.FieldName == fieldName))
						{
							var refAtt = schema.Attributes.FirstOrDefault(a => a.FieldName == m.FieldName);
							var newAtt = new JMXAttribute(fieldName)
							{
								DataType = refAtt.DataType,
								ServerDataType = refAtt.ServerDataType,
								Required = true,
								FieldName = fieldName,
								IsFK = true,
								DataSize = refAtt.DataSize
							};
							att.ObjectSchema.Attributes.Add(newAtt);
						}
					}
					//check for exists attribute $"{schema.ObjectName}{m.FieldName}" in att.ObjectSchema
					fk.AddKeyMember(schema.PrimaryKey.KeyMembers.Select(m => $"{schema.ObjectName}{m.FieldName}").ToArray());
					fk.AddRefKeyMember(schema.PrimaryKey.KeyMembers.Select(m => m.FieldName).ToArray());
					att.ObjectSchema.ForeignKeys.Add(fk);
				}
			}
		}

		#endregion Save Schema

		#region Utils
		public override IEnumerable<string> GetChildObjects(string objectName)
		{
			throw new NotImplementedException();
		}

		public override Task<JMXSchema> SetSchemaStateAsync(string objectName, int stateId)
		{
			//var mdb = Factory.GetMdbContext();
			var schema = GetSchemaAsync(objectName);
			//??? разобраться
			//await mdb.ExecuteAsync($@"update SysCat.SysSchemas 
			//			set ObjectSchema = @ObjectSchema,
			//			State = 1 where id = {schema.ID}",
			//	new MdbParameter("@ObjectSchema", schema.ToString()));
			return schema;
		}
		#endregion Utils
	}
}