using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Data;
using S031.MetaStack.Json;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public class JMXSqlRepo : JMXRepo
	{
		const int _sql_server_2017_version = 14;
		static int _counter = 0;
		static readonly MapTable<string, JMXSchema> _schemaCache = new MapTable<string, JMXSchema>();

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
		const string detail_field_prefix = "$1_";

		/// <summary>
		/// Вопрос, а надо ли статик?
		/// </summary>
		private static string _defaultDbSchema = string.Empty;
		private static string _sqlVersion = string.Empty;
		private static readonly JMXSQLTypeMapping _typeMapping = new JMXSQLTypeMapping();
		private static IReadOnlyDictionary<MdbType, string> TypeMap => _typeMapping.GetTypeMap();
		private static IReadOnlyDictionary<string, MdbTypeInfo> TypeInfo => _typeMapping.GetServerTypeMap();

		public JMXSqlRepo(JMXSqlFactory factory) : base(factory)
		{
			TestSysCat();
		}

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
			schema = await GetSchemaAsync(this.GetMdbContext(ContextTypes.SysCat), name.AreaName, name.ObjectName)
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
			var mdb = this.GetMdbContext(ContextTypes.SysCat);
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
				_defaultDbSchema = await this
					.GetMdbContext(ContextTypes.SysCat)
					.ExecuteAsync<string>(SqlServer.GetDefaultSchema);
				if (_defaultDbSchema.IsEmpty())
					_defaultDbSchema = await this
						.GetMdbContext(ContextTypes.Work)
						.ExecuteAsync<string>("select schema_name() as Name");
			}
			return _defaultDbSchema;
		}

		private async Task CreateDbSchemaAsync()
		{
			MdbContext mdb = this.GetMdbContext(ContextTypes.SysCat);
			try
			{
				await mdb.BeginTransactionAsync();
				var scripts = (await IsSql17(mdb)) ?
					SqlServer.CreateSchemaObjects.Split(new string[] { "--go", "--GO" },
						StringSplitOptions.RemoveEmptyEntries) :
					SqlServer.CreateSchemaObjects_12.Split(new string[] { "--go", "--GO" },
						StringSplitOptions.RemoveEmptyEntries);

				foreach (string statement in scripts)
					await mdb.ExecuteAsync(statement);
				Logger.Debug($"Schema SysCat was created in database {mdb.DbName}");
				Logger.Debug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
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

		#region Drop Schema
		public override void DropSchema(string objectName)
		{
			DropSchemaAsync(objectName).GetAwaiter().GetResult();
		}

		public override async Task DropSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			await DropSchemaAsync(name.AreaName, name.ObjectName).ConfigureAwait(false);
			_schemaCache.Remove(name.ToString());
		}

		private async Task DropSchemaAsync(string areaName, string objectName)
		{
			MdbContext mdb = this.GetMdbContext(ContextTypes.SysCat);
			ILogger logger = this.Logger;
			var schema = await GetSchemaAsync(mdb, areaName, objectName);
			var schemaFromDb = await GetTableSchemaAsync(
				this.GetMdbContext(ContextTypes.Work), 
				schema.DbObjectName.ToString());

			string[] sqlList;
			if (schemaFromDb == null)
				sqlList = new string[] { };
			else
				sqlList = await DropSchemaStatementsAsync(schemaFromDb);

			if (sqlList.Length > 0)
			{
				await mdb.BeginTransactionAsync();
				try
				{
					foreach (var sql in sqlList)
					{
						logger.Debug(sql);
						await mdb.ExecuteAsync(sql);
					}
					await mdb.ExecuteAsync(SqlServer.DelSysSchemas,
						new MdbParameter("@uid", schema.UID));
					await mdb.CommitAsync();
				}
				catch
				{
					mdb.RollBack();
					throw;
				}
			}

		}

		private async Task<string[]> DropSchemaStatementsAsync(JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(_typeMapping, fromDbSchema))
			{
				await WriteDropStatementsAsync(sb, fromDbSchema);
				sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task WriteDropStatementsAsync(SQLStatementWriter sb, JMXSchema fromDbSchema)
		{
			MdbContext mdb = this.GetMdbContext();
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(detail_field_prefix)))
			{
				string[] names = att.FieldName.Split('_');
				var schemaFromDb = await GetTableSchemaAsync(mdb, new JMXObjectName(names[1], names[2]).ToString());
				if (schemaFromDb != null)
					await WriteDropStatementsAsync(sb, schemaFromDb);

			}
			//string s = (await mdb.ExecuteAsync<string>(
			//	(await IsSql17(mdb)) ? SqlServer.GetParentRelations : SqlServer.GetParentRelations_12,
			//	new MdbParameter("@table_name", fromDbSchema.DbObjectName.ToString()))) ?? "";
			sb.WriteDropStatements(fromDbSchema);
		}

		#endregion Drop Schema

		#region Clear Catalog
		public override void ClearCatalog()
		{
			DropDbSchemaAsync(this.GetMdbContext(), this.Logger)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
		public override async Task ClearCatalogAsync()
		{
			await DropDbSchemaAsync(this.GetMdbContext(), this.Logger).ConfigureAwait(false);
		}
		private async Task DropDbSchemaAsync(MdbContext mdb, ILogger log)
		{
			var test = await mdb.ExecuteAsync<string>(SqlServer.TestSchema);
			if (!test.IsEmpty())
			{
				try
				{
					await mdb.ExecuteAsync(SqlServer.DropSchema);
					log.Debug($"Schema SysCat was deleted from database {mdb.DbName}");
				}
				catch (Exception e)
				{
					log.LogError($"Delete Schema error: {e.Message}");
					throw;
				}
			}
			else
				log.Debug($"Schema SysCat not exists in database  {mdb.DbName}");
		}
		#endregion Clear Catalog

		#region Save Schema
		public override JMXSchema SaveSchema(JMXSchema schema)
			=> SaveSchemaAsync(schema)
			.GetAwaiter()
			.GetResult();

		public override async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			var mdb = this.GetMdbContext();
			schema = await NormalizeSchemaAsync(mdb, schema);
			int id = (await IsSql17(mdb)) ?
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

				string typeMap = TypeMap[att.DataType];
				if (att.ServerDataType.IsEmpty() || typeMap.IndexOf(att.ServerDataType) == -1)
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
				att.FieldName = $"{detail_field_prefix}{att.ObjectSchema.DbObjectName.AreaName}_{att.ObjectSchema.DbObjectName.ObjectName}";
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

		#region Sync Schema
		public override async Task<JMXSchema> SyncSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			var schema = await SyncSchemaAsync(name.AreaName, name.ObjectName).ConfigureAwait(false);
			_schemaCache[schema.ObjectName] = schema;
			return schema;
		}
		public async Task<JMXSchema> SyncSchemaAsync(string dbSchema, string objectName)
		{
			MdbContext mdb = this.GetMdbContext();
			ILogger log = this.Logger;
			var schema = await GetSchemaInternalAsync(mdb, dbSchema, objectName, 0);
			//schema was syncronized
			if (schema.SyncState == 1)
				return schema;

			foreach (var o in GetDependences(schema))
				await SyncSchemaAsync(o.AreaName, o.ObjectName);

			var schemaFromDb = await GetTableSchemaAsync(mdb, schema.DbObjectName.ToString());
			bool createNew = (schemaFromDb == null);
			string[] sqlList;
			if (createNew)
				sqlList = CreateNewStatements(schema);
			else
			{
				// Compare with previos version of schema
				// error schema not found if db objects exists, but 
				// synced version schema don't exists
				var prevSchema = await GetSchemaInternalAsync(mdb, dbSchema, objectName, 1);
				sqlList = await CompareSchemasAsync(schema, prevSchema);
			}

			await mdb.BeginTransactionAsync();
			try
			{
				foreach (var sql in sqlList)
				{
					log.Debug(sql);
					await mdb.ExecuteAsync(sql);
				}
				//if (!createNew)
				//	await schemaUpdate(mdb, schema);
				await mdb.ExecuteAsync($"update SysCat.SysSchemas set ObjectSchema = @ObjectSchema where id = {schema.ID}",
					new MdbParameter("@ObjectSchema", schema.ToString()));
				await mdb.ExecuteAsync(SqlServer.StateSysSchemas,
					new MdbParameter("@id", schema.ID));
				await mdb.CommitAsync();
			}
			catch
			{
				mdb.RollBack();
				throw;
			}
			return schema;
		}
		private string[] CreateNewStatements(JMXSchema schema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(_typeMapping, schema))
			{
				sb.WriteCreateNewTableStatements();
				sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task<string[]> CompareSchemasAsync(JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(_typeMapping, schema))
			{
				await CompareSchemasStatementsAsync(sb, schema, fromDbSchema);
				string stmt = sb.ToString();
				if (stmt.Length > 1)
					sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task CompareSchemasStatementsAsync(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			MdbContext mdb = this.GetMdbContext();
			bool recreate = false;
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(detail_field_prefix)))
			{
				if (!schema.Attributes.Any(a => a.FieldName == att.FieldName))
				{
					string[] names = att.FieldName.Split('_');
					var schemaFromDb = await GetTableSchemaAsync(mdb, new JMXObjectName(names[1], names[2]).ToString());
					if (schemaFromDb != null)
						await WriteDropStatementsAsync(sb, schemaFromDb);
				}

			}

			recreate = CompareAttributes(sb, schema, fromDbSchema);
			if (!recreate)
				recreate = ComparePK(sb, schema, fromDbSchema);

			if (!recreate)
			{
				CompareIndexes(sb, schema, fromDbSchema);
				CompareFK(sb, schema, fromDbSchema);
			}
			else
				await RecreateSchemaAsync(mdb, sb, schema, fromDbSchema);

			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
			{
				var schemaFromDb = await GetTableSchemaAsync(mdb, att.ObjectSchema.DbObjectName.ToString());
				if (schemaFromDb != null)
					await CompareSchemasStatementsAsync(sb, att.ObjectSchema, schemaFromDb);
				else
					sb.WriteCreateNewTableStatements(att.ObjectSchema);

			}
		}

		private static bool CompareAttributes(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			bool recreate = false;
			int count = schema.Attributes.Count;
			List<(JMXAttribute att, JMXAttribute att2, AttribCompareDiff diff)> l =
				new List<(JMXAttribute, JMXAttribute, AttribCompareDiff)>();
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				var att2 = fromDbSchema.Attributes.FirstOrDefault(a =>
					a.FieldName.Equals(att.FieldName, StringComparison.OrdinalIgnoreCase));

				bool found = (att2 != null);
				AttribCompareDiff diff = AttribCompareDiff.none;
				if (found)
				{
					if (!att.ServerDataType.Equals(att2.ServerDataType, StringComparison.OrdinalIgnoreCase))
						diff = AttribCompareDiff.dataTtype;
					if (att.Required != att2.Required)
						diff |= AttribCompareDiff.nullOptions;
					if (att.Identity.IsIdentity != att2.Identity.IsIdentity ||
						att.Identity.Seed != att2.Identity.Seed ||
						att.Identity.Increment != att2.Identity.Increment)
						diff |= AttribCompareDiff.identity;

					if (att.DataType != MdbType.@object &&
						!att.AttribName.Equals(att2.FieldName, StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.name;

					//Server DataTypes is equals
					if ((diff & AttribCompareDiff.dataTtype) != AttribCompareDiff.dataTtype)
					{
						MdbTypeInfo ti = TypeInfo[att.ServerDataType];
						if (!ti.FixedSize)
						{
							if (_typeMapping.GetVariableLenghtDataTypes().Contains(att.ServerDataType) && att.DataSize.Size != att2.DataSize.Size)
								diff |= AttribCompareDiff.size;
							else if (ti.MdbType == MdbType.@decimal && (att.DataSize.Precision != att2.DataSize.Precision ||
								att.DataSize.Scale != att2.DataSize.Scale))
								diff |= AttribCompareDiff.size;
						}
					}
					if (!att.CheckConstraint
						.Definition
						.RemoveChar("[( )]".ToCharArray())
						.Equals(att2.CheckConstraint
							.Definition
							.RemoveChar("[( )]".ToCharArray()),
						StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.constraint;
					else if (!att.DefaultConstraint
						.Definition.RemoveChar("[( )]".ToCharArray())
						.Equals(att2.DefaultConstraint
							.Definition
							.RemoveChar("[( )]".ToCharArray()),
						StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.constraint;
				}
				//else if (att2 != null)
				//	diff |= AttribCompareDiff.name;
				else
					diff = AttribCompareDiff.notFound;
				l.Add((att, att2, diff));
			}
			foreach (var att2 in fromDbSchema.Attributes)
			{
				//var att = schema.Attributes.FirstOrDefault(a => a.ID == att2.ID);
				var att = schema.Attributes.FirstOrDefault(a =>
					a.FieldName.Equals(att2.FieldName, StringComparison.OrdinalIgnoreCase));
				if (att == null)
					l.Add((att, att2, AttribCompareDiff.remove));
			}

			foreach (var (att, att2, diff) in l)
			{
				if ((diff & AttribCompareDiff.remove) == AttribCompareDiff.remove)
				{
					sb.WriteDropColumnStatement(att2);
					continue;
				}
				if ((diff & AttribCompareDiff.constraint) == AttribCompareDiff.constraint)
				{
					sb.WriteDropConstraintStatement(att2, fromDbSchema);
					sb.WriteCreateConstraintStatement(att);
				}
				if ((diff & AttribCompareDiff.dataTtype) == AttribCompareDiff.dataTtype ||
					(diff & AttribCompareDiff.size) == AttribCompareDiff.size ||
					(diff & AttribCompareDiff.nullOptions) == AttribCompareDiff.nullOptions)
					sb.WriteAlterColumnStatement(att);
				else if ((diff & AttribCompareDiff.notFound) == AttribCompareDiff.notFound)
					sb.WriteAlterColumnStatement(att, true);
				else if ((diff & AttribCompareDiff.name) == AttribCompareDiff.name)
				{
					att.FieldName = att.AttribName;
					sb.WriteRenameColumnStatement(att2.FieldName, att.FieldName);
				}
				else if ((diff & AttribCompareDiff.identity) == AttribCompareDiff.identity)
					recreate = true;
			}
			return recreate;
		}

		private static bool ComparePK(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			///Add PK compare
			///writeDropPKStatement
			///writeCreatePKStatement
			///The constraint 'XPK1Requests' is being referenced by table 'PaymentStateHists', 
			///foreign key constraint 'FK_PAYMENTSTATEHISTS_REQUESTS'.
			///Could not drop constraint. See previous errors.
			if (schema.PrimaryKey == null && fromDbSchema.PrimaryKey != null)
				//writeDropPKStatement(sb, fromDbSchema);
				return true;
			else if (schema.PrimaryKey != null && fromDbSchema.PrimaryKey == null)
				sb.WriteCreatePKStatement();
			else if (schema.PrimaryKey != null &&
				schema.PrimaryKey.KeyName != fromDbSchema.PrimaryKey.KeyName ||
				schema.PrimaryKey.KeyMembers == fromDbSchema.PrimaryKey.KeyMembers)
			{
				//writeDropPKStatement(sb, fromDbSchema);
				//writeCreatePKStatement(sb, schema);
				return true;
			}
			return false;
		}

		private static void CompareIndexes(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<(JMXIndex i1, JMXIndex i2, DbObjectOnDiffActions action)> l =
				new List<(JMXIndex i1, JMXIndex i2, DbObjectOnDiffActions action)>();
			int count = schema.Indexes.Count;
			for (int i = 0; i < count; i++)
			{
				var i1 = schema.Indexes[i];
				var i2 = fromDbSchema.Indexes.FirstOrDefault(index => index.IndexName == i1.IndexName);
				if (i2 == null)
					l.Add((i1, i2, DbObjectOnDiffActions.add));
				else if (i1.ClusteredOption != i2.ClusteredOption)
					l.Add((i1, i2, DbObjectOnDiffActions.alter));
				else if (i1.IsUnique != i2.IsUnique)
					l.Add((i1, i2, DbObjectOnDiffActions.alter));
				else
				{
					foreach (var m in i1.KeyMembers)
					{
						var m2 = i2.KeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((i1, i2, DbObjectOnDiffActions.alter));
							break;
						}

					}
				}
			}
			count = fromDbSchema.Indexes.Count;
			for (int i = 0; i < count; i++)
			{
				var i2 = fromDbSchema.Indexes[i];
				var i1 = schema.Indexes.FirstOrDefault(index => index.IndexName == i2.IndexName);
				if (i1 == null)
					l.Add((i1, i2, DbObjectOnDiffActions.drop));
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.drop)
					sb.WriteDropIndexStatement(i2, fromDbSchema);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					sb.WriteCreateIndexStatement(i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					sb.WriteDropIndexStatement(i2, fromDbSchema);
					sb.WriteCreateIndexStatement(i1);
				}
			}

		}

		private static void CompareFK(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<(JMXForeignKey i1, JMXForeignKey i2, DbObjectOnDiffActions action)> l =
				new List<(JMXForeignKey i1, JMXForeignKey i2, DbObjectOnDiffActions action)>();
			int count = schema.ForeignKeys.Count;
			for (int i = 0; i < count; i++)
			{
				var k1 = schema.ForeignKeys[i];
				var k2 = fromDbSchema.ForeignKeys.FirstOrDefault(fk => fk.KeyName == k1.KeyName);
				if (k2 == null)
					l.Add((k1, k2, DbObjectOnDiffActions.add));
				else if (k1.CheckOption != k2.CheckOption ||
					k1.DeleteRefAction != k2.DeleteRefAction ||
					k1.UpdateRefAction != k2.UpdateRefAction ||
					!k1.RefDbObjectName.ToString().Equals(k2.RefDbObjectName.ToString(), StringComparison.OrdinalIgnoreCase))
					l.Add((k1, k2, DbObjectOnDiffActions.alter));
				else
				{
					foreach (var m in k1.KeyMembers)
					{
						var m2 = k2.KeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((k1, k2, DbObjectOnDiffActions.alter));
							break;
						}

					}
					foreach (var m in k1.RefKeyMembers)
					{
						var m2 = k2.RefKeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((k1, k2, DbObjectOnDiffActions.alter));
							break;
						}

					}
				}
			}

			count = fromDbSchema.ForeignKeys.Count;
			for (int i = 0; i < count; i++)
			{
				var k2 = fromDbSchema.ForeignKeys[i];
				var k1 = schema.ForeignKeys.FirstOrDefault(fk => fk.KeyName == k2.KeyName);
				if (k1 == null)
					l.Add((k1, k2, DbObjectOnDiffActions.drop));
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.drop)
					sb.WriteDropFKStatement(i2, fromDbSchema);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					sb.WriteCreateFKStatement(i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					sb.WriteDropFKStatement(i2, fromDbSchema);
					sb.WriteCreateFKStatement(i1);
				}
			}
		}

		private static async Task RecreateSchemaAsync(MdbContext mdb, SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			int recCount = (await mdb.ExecuteAsync<int>($"select count(*) from {fromDbSchema.DbObjectName.ToString()}"));

			foreach (var fk in fromDbSchema.ParentRelations)
				sb.WriteDropParentRelationStatement(fk);

			foreach (var fk in fromDbSchema.ForeignKeys)
				sb.WriteDropFKStatement(fk, fromDbSchema);

			string tmpTableName = fromDbSchema.DbObjectName.ObjectName + "_" + DateTime.Now.Subtract(vbo.Date()).Seconds.ToString();
			if (recCount > 0)
				sb.WriteRenameTableStatement(tmpTableName, fromDbSchema);
			else
				sb.WriteDropTableStatement(null, fromDbSchema);

			sb.WriteCreateTableStatements(schema);

			if (recCount > 0)
			{
				sb.WriteInsertRowsStatement(tmpTableName, schema);
				sb.WriteDropTableStatement(tmpTableName, fromDbSchema);
			}

			sb.WriteCreatePKStatement(schema);
			foreach (var att in schema.Attributes)
				sb.WriteCreateConstraintStatement(att, schema);
			foreach (var index in schema.Indexes)
				sb.WriteCreateIndexStatement(index, schema);
			foreach (var fk in schema.ForeignKeys)
				sb.WriteCreateFKStatement(fk, schema);

			/// Need test
			foreach (var fk in fromDbSchema.ParentRelations)
				sb.WriteCreateParentRelationStatement(fk);
		}

		private static string GetDiffs(AttribCompareDiff diff)
		{
			List<string> l = new List<string>();
			if (((diff & AttribCompareDiff.notFound) == AttribCompareDiff.notFound))
				l.Add(AttribCompareDiff.notFound.ToString());
			if (((diff & AttribCompareDiff.name) == AttribCompareDiff.name))
				l.Add(AttribCompareDiff.name.ToString());
			if (((diff & AttribCompareDiff.dataTtype) == AttribCompareDiff.dataTtype))
				l.Add(AttribCompareDiff.dataTtype.ToString());
			if (((diff & AttribCompareDiff.size) == AttribCompareDiff.size))
				l.Add(AttribCompareDiff.size.ToString());
			if (((diff & AttribCompareDiff.nullOptions) == AttribCompareDiff.nullOptions))
				l.Add(AttribCompareDiff.nullOptions.ToString());
			if (((diff & AttribCompareDiff.identity) == AttribCompareDiff.identity))
				l.Add(AttribCompareDiff.identity.ToString());
			if (l.Count == 0)
				l.Add(AttribCompareDiff.none.ToString());
			return string.Join('\t', l.ToArray());
		}

		#endregion Sync Schema

		#region Utils
		public override async Task<JMXSchema> GetTableSchemaAsync(string objectName)=>
			await GetTableSchemaAsync(GetMdbContext(ContextTypes.Work), objectName);

		private static async Task<JMXSchema> GetTableSchemaAsync(MdbContext mdb, string fullTableName)
		{
			string s = await mdb.ExecuteAsync<string>(
				(await IsSql17(mdb)) ? SqlServer.GetTableSchema : SqlServer.GetTableSchema_12,
				new MdbParameter("@table_name", fullTableName));
			if (s != null)
				return JMXSchema.Parse(s);
			return null;
		}
		private static async Task<string> GetSqlVersion(MdbContext mdb)
		{
			if (_sqlVersion.IsEmpty())
			{
				_sqlVersion = await mdb.ExecuteAsync<string>(SqlServer.SQLVersion);
				if (_sqlVersion.IsEmpty())
					_sqlVersion = "10";
				else
					_sqlVersion = _sqlVersion.GetToken(0, ".");
			}
			return _sqlVersion;
		}

		private static async Task<bool> IsSql17(MdbContext mdb)
			=> (await GetSqlVersion(mdb)).ToIntOrDefault() >= _sql_server_2017_version;

		private static JMXObjectName[] GetDependences(JMXSchema schema)=>
			schema.ForeignKeys.Select(fk => fk.RefDbObjectName).ToArray();
		#endregion Utils
	}
}
