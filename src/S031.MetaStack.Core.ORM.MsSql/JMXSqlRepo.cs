using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public sealed class JMXSqlRepo : JMXRepo
	{
		static int _counter = 0;
		static readonly MapTable<string, JMXSchema> _schemaCache = new MapTable<string, JMXSchema>();

		const string schema_version = "0.0.1";

		/// <summary>
		/// Вопрос, а надо ли статик?
		/// </summary>
		private static string _defaultDbSchema = string.Empty;

		public JMXSqlRepo(JMXSqlFactory factory) : base(factory)
		{
			TestSysCat();
		}

		public ILogger Logger => Factory.Logger;

		#region GetSchema
		public override JMXSchema GetSchema(string objectName)=> GetSchemaAsync(objectName).GetAwaiter().GetResult();
		public override async Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			if (_schemaCache.TryGetValue(name, out JMXSchema schema))
			{
				schema.SchemaRepo = this;
				return schema;
			}
			schema = await GetSchemaAsync(Factory.GetMdbContext(), name.AreaName, name.ObjectName)
				.ConfigureAwait(false);
			_schemaCache.TryAdd(name, schema);
			return schema;
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

		private async Task TestSysCatAsync()
		{
			var mdb = Factory.GetMdbContext();
			var result = await mdb.ExecuteAsync<string>(SqlServer.TestSchema);
			if (result.IsEmpty())
			{
				await CreateDbSchemaAsync();
				_defaultDbSchema = await GetDefaultDbSchemaAsync();
			}
		}
		private async Task<string> GetDefaultDbSchemaAsync()
		{
			if (_defaultDbSchema.IsEmpty())
			{
				_defaultDbSchema = await Factory
					.GetMdbContext()
					.ExecuteAsync<string>(SqlServer.GetDefaultSchema);
				if (_defaultDbSchema.IsEmpty())
					_defaultDbSchema = await Factory
						.GetMdbContext()
						.ExecuteAsync<string>("select schema_name() as Name");
			}
			return _defaultDbSchema;
		}

		private async Task CreateDbSchemaAsync()
		{
			MdbContext mdb = Factory.GetMdbContext();
			try
			{
				await mdb.BeginTransactionAsync();
				var scripts = (await SqlServerHelper.IsSql17(mdb)) ?
					SqlServer.CreateSchemaObjects.Split(new string[] { "--go", "--GO" },
						StringSplitOptions.RemoveEmptyEntries) :
					SqlServer.CreateSchemaObjects_12.Split(new string[] { "--go", "--GO" },
						StringSplitOptions.RemoveEmptyEntries);

				foreach (string statement in scripts)
					await mdb.ExecuteAsync(statement);
				Logger.LogDebug($"Schema SysCat was created in database {mdb.DbName}");
				Logger.LogDebug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
				string owner = await GetDefaultDbSchemaAsync();
				int id = await mdb.ExecuteAsync<int>(SqlServer.AddSysAreas,
					new MdbParameter("@SchemaName", owner),
					new MdbParameter("@SchemaOwner", owner),
					new MdbParameter("@SchemaVersion", schema_version));
				id = await mdb.ExecuteAsync<int>(SqlServer.AddSysAreas,
					new MdbParameter("@SchemaName", "SysCat"),
					new MdbParameter("@SchemaOwner", owner),
					new MdbParameter("@SchemaVersion", schema_version));
				await mdb.CommitAsync();
			}
			catch (Exception e)
			{
				mdb.RollBack();
				Logger.LogError($"CreateSchema error: {e.Message}");
				throw;
			}
		}

		private static async Task<JMXSchema> GetSchemaAsync(MdbContext mdb, string areaName, string objectName)
		{
			string sql = $"select Top 1 ID, ObjectSchema, SyncState from SysCat.V_SysSchemas where SchemaName = '{areaName}' and " +
				$"(ObjectName = '{objectName}' or DbObjectName = '{objectName}') and SyncState >= 0 " +
				$"order by SyncState desc";
			using (var dr = await mdb.GetReaderAsync(sql))
			{
				if (!dr.Read())
					//object schema not found in database
					throw new InvalidOperationException(
						string.Format(Properties.Strings.S031_MetaStack_Core_SysCat_SysCatManager_getSchema_1, $"{areaName}.{objectName}"));
				var schema = JMXSchema.Parse((string)dr["ObjectSchema"]);
				schema.ID = (int)dr["ID"];
				schema.SyncState = (int)dr["SyncState"];
				return schema;
			}
		}

		private static async Task<JMXSchema> GetSchemaInternalAsync(MdbContext mdb, string areaName, string objectName, int syncState)
		{
			string sql = $"select Top 1 ID, ObjectSchema, SyncState from SysCat.V_SysSchemas where SchemaName = '{areaName}' and " +
				$"(ObjectName = '{objectName}' or DbObjectName = '{objectName}') and SyncState >= {syncState}" +
				$"order by SyncState";
			using (var dr = await mdb.GetReaderAsync(sql))
			{
				if (!dr.Read())
					//object schema not found in database
					throw new InvalidOperationException(
						string.Format(Properties.Strings.S031_MetaStack_Core_SysCat_SysCatManager_getSchema_2,
						$"{areaName}.{objectName}", syncState));
				var schema = JMXSchema.Parse((string)dr["ObjectSchema"]);
				schema.ID = (int)dr["ID"];
				schema.SyncState = (int)dr["SyncState"];
				return schema;
			}
		}
		#endregion GetSchema

		#region Delete Schema
		public override void DeleteSchema(string objectName)
			=> DeleteSchemaAsync(objectName).GetAwaiter().GetResult();

		public override async Task DeleteSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			MdbContext mdb = Factory.GetMdbContext();
			var schema = await GetSchemaAsync(mdb, name.AreaName, name.ObjectName);
			
			await mdb.ExecuteAsync(SqlServer.DelSysSchemas,
				new MdbParameter("@uid", schema.UID));
			_schemaCache.Remove(name.ToString());
		}
		#endregion Delete Schema

		#region Clear Catalog
		public override void ClearCatalog()
		{
			DropDbSchemaAsync(Factory.GetMdbContext(), this.Logger)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
		public override async Task ClearCatalogAsync()
		{
			await DropDbSchemaAsync(Factory.GetMdbContext(), this.Logger).ConfigureAwait(false);
		}
		private static async Task DropDbSchemaAsync(MdbContext mdb, ILogger log)
		{
			var test = await mdb.ExecuteAsync<string>(SqlServer.TestSchema);
			if (!test.IsEmpty())
			{
				try
				{
					await mdb.ExecuteAsync(SqlServer.DropSchema);
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
		public override JMXSchema SaveSchema(JMXSchema schema)
			=> SaveSchemaAsync(schema)
			.GetAwaiter()
			.GetResult();

		public override async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			var mdb = Factory.GetMdbContext();
			schema = await NormalizeSchemaAsync(mdb, schema);
			int id = (await SqlServerHelper.IsSql17(mdb)) ?
				await mdb.ExecuteAsync<int>(SqlServer.AddSysSchemas,
					new MdbParameter("@ObjectSchema", schema.ToString()),
					new MdbParameter("@Version", schema_version)) :
				await mdb.ExecuteAsync<int>(SqlServer.AddSysSchemas,
					new MdbParameter("@uid", schema.UID),
					new MdbParameter("@SysAreaSchemaName", schema.ObjectName.AreaName),
					new MdbParameter("@ObjectType", (int)schema.DbObjectType),
					new MdbParameter("@ObjectName", schema.ObjectName.ObjectName),
					new MdbParameter("@DbObjectName", schema.DbObjectName.ObjectName),
					new MdbParameter("@ObjectSchema", schema.ToString()),
					new MdbParameter("@Version", schema_version));

			schema.ID = id;
			_schemaCache[schema.ObjectName] = schema;
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

				string typeMap = SqlServerHelper.TypeMap[att.DataType];
				if (att.ServerDataType.IsEmpty() || !typeMap.Contains(att.ServerDataType, StringComparison.CurrentCulture))
					att.ServerDataType = typeMap.GetToken(0, ";");

				if (att.DataSize.IsEmpty())
				{
					MdbTypeInfo ti2 = SqlServerHelper.TypeInfo[att.ServerDataType];
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
						throw new InvalidOperationException(string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3,
							member.FieldName, "primary key"));
				}
			}
			else if (schema.Attributes.Any(a => a.DataType == MdbType.@object))
			{
                //Requires PK for table with object attributes
                var pk = new JMXPrimaryKey(keyName: $"PK_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}");
				var att = schema.Attributes.FirstOrDefault(a => a.Identity.IsIdentity);
				if (att == null)
					att = schema.Attributes.FirstOrDefault(a => a.AttribName.Equals("ID", StringComparison.OrdinalIgnoreCase) 
					|| a.FieldName.Equals("ID", StringComparison.OrdinalIgnoreCase));
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
					throw new InvalidOperationException(string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_5,
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
						throw new InvalidOperationException(string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3,
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
						throw new ArgumentNullException(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_6);
					// set value, not ref
					fk.RefObjectName = refSchema.ObjectName.ToString();
				}

				foreach (var member in fk.KeyMembers)
				{
					var att = schema.Attributes.FirstOrDefault(a => a.FieldName == member.FieldName);
					if (att == null)
						//The FieldName specified in the foreign key is not in the attribute list
						throw new InvalidOperationException(
							string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchemaProviderDB_normalize_3, member.FieldName, 
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

		public async override Task<JMXSchema> SetSchemaStateAsync(string objectName, int stateId)
		{
			var mdb = Factory.GetMdbContext();
			var schema = await GetSchemaAsync(objectName);
			//!!! ???
			//await mdb.ExecuteAsync($"update SysCat.SysSchemas set ObjectSchema = @ObjectSchema where id = {schema.ID}",
			//	new MdbParameter("@ObjectSchema", schema.ToString()));
			await mdb.ExecuteAsync(SqlServer.StateSysSchemas,
				new MdbParameter("@id", schema.ID));
			return schema;
		}

		#region Utils
		public override IEnumerable<string> GetChildObjects(string objectName)
		{
			throw new NotImplementedException();
		}

		#endregion Utils
	}
}
