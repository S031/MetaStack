using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;

namespace S031.MetaStack.Core.ORM
{
	public class JMXSqlRepo : JMXRepo
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
		const string detail_field_prefix = "$1_";
		private static readonly Dictionary<MdbType, string> _typeMap = new Dictionary<MdbType, string>()
		{
			{MdbType.@null, "int" },
			{MdbType.@bool, "bit" },
			{MdbType.@char, "tinyint" },
			{MdbType.@byte, "tinyint" },
			{MdbType.@sbyte, "tinyint" },
			{MdbType.@short, "smallint" },
			{MdbType.@ushort, "smallint" },
			{MdbType.@int, "int" },
			{MdbType.@uint, "int" },
			{MdbType.@long, "bigint" },
			{MdbType.@ulong, "bigint" },
			{MdbType.@float, "float;real" },
			{MdbType.@double, "float;real" },
			{MdbType.@decimal, "decimal;money;numeric;smallmoney" },
			{MdbType.dateTime, "date;datetime;datetime2;smalldatetime;time" },
			{MdbType.@string, "varchar;nvarchar;text;ntext;char;nchar;xml" },
			{MdbType.byteArray, "binary;varbinary;image;rowversion;timestamp;sql_variant" },
			{MdbType.charArray, "char;nchar" },
			{MdbType.guid, "uniqueidentifier" },
			{MdbType.@object, "varchar" }
		};
		const string variable_lenght_data_types = "varchar;nvarchar;char;nchar;varbinary";

		private static readonly Dictionary<string, MdbTypeInfo> _typeInfo = new Dictionary<string, MdbTypeInfo>()
		{
			{ "bit", new MdbTypeInfo() { MdbType = MdbType.@bool, Type = typeof(bool), Name = "bit", Size = 1, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = false} },
			{ "tinyint", new MdbTypeInfo() { MdbType = MdbType.@byte, Type = typeof(byte), Name = "tinyint", Size = 1, Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true} },
			{ "smallint", new MdbTypeInfo() { MdbType = MdbType.@short, Type = typeof(short), Name = "smallint", Size = 2, Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true} },
			{ "int", new MdbTypeInfo() { MdbType = MdbType.@int, Type = typeof(int), Name = "int", Size = 4, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ "bigint", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "bigint", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ "real", new MdbTypeInfo() { MdbType = MdbType.@double, Type = typeof(double), Name = "real", Size = 4, Scale = 0, Precision = 24, FixedSize = true, NullIfEmpty = true} },
			{ "float", new MdbTypeInfo() { MdbType = MdbType.@float, Type = typeof(float), Name = "float", Size = 8, Scale = 0, Precision = 53, FixedSize = true, NullIfEmpty = true} },
			{ "time", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "time", Size = 5, Scale = 7, Precision = 16, FixedSize = true, NullIfEmpty = true} },
			{ "date", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "date", Size = 3, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ "datetime", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime", Size = 8, Scale = 3, Precision = 23, FixedSize = true, NullIfEmpty = true} },
			{ "datetime2", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime2", Size = 8, Scale = 7, Precision = 27, FixedSize = true, NullIfEmpty = true} },
			{ "smalldatetime", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "smalldatetime", Size = 4, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true} },
			{ "timestamp", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "timestamp", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ "smallmoney", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "smallmoney", Size = 4, Scale = 4, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ "money", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "money", Size = 8, Scale = 4, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ "numeric", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "numeric", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true} },
			{ "decimal", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "decimal", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true} },
			{ "text", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "text", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "ntext", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "ntext", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "varchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "varchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "nvarchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nvarchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "char", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "char", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "nchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "xml", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "xml", Size = -1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "binary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "binary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "varbinary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "varbinary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "image", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "image", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "uniqueidentifier", new MdbTypeInfo() { MdbType = MdbType.guid, Type = typeof(Guid), Name = "uniqueidentifier", Size = 16, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true} },
			{ "sql_variant", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "sql_variant", Size = 8016, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
		};

		/// <summary>
		/// Вопрос, а надо ли статик?
		/// </summary>
		private static string _defaultDbSchema = string.Empty;

		private static string _sqlVersion = string.Empty;

		public JMXSqlRepo(MdbContext mdbContext, ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(JMXSqlFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSqlFactory.ProviderInvariantName} provider.");
			Logger = logger;
			TestSysCatAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		public override IDictionary<MdbType, string> GetTypeMap() => _typeMap;

		public override IDictionary<string, MdbTypeInfo> GetServerTypeMap() => _typeInfo;

		public override string[] GetVariableLenghtDataTypes() => variable_lenght_data_types.Split(';');
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
			schema = await GetSchemaAsync(this.MdbContext, name.AreaName, name.ObjectName).ConfigureAwait(false);
			lock (objLock)
			{
				if (!_schemaCache.ContainsKey(name))
					_schemaCache.Add(name, schema);
			}
			return schema;
		}

		private async Task TestSysCatAsync()
		{
			if (_counter == 0)
			{
				if (!(await this.MdbContext.ExecuteAsync<string>(SqlServer.TestSchema)).IsEmpty())
				{
					await CreateDbSchemaAsync(this.MdbContext, this.Logger);
					_defaultDbSchema = await GetDefaultDbSchemaAsync(this.MdbContext);
					Interlocked.Increment(ref _counter);
				}
			}
		}

		private static async Task<string> GetDefaultDbSchemaAsync(MdbContext mdb)
		{
			if (_defaultDbSchema.IsEmpty())
				_defaultDbSchema = await mdb.ExecuteAsync<string>(SqlServer.GetDefaultSchema);
			return _defaultDbSchema;
		}

		private static async Task CreateDbSchemaAsync(MdbContext mdb, ILogger log)
		{
			try
			{
				await mdb.BeginTransactionAsync();
				var scripts = SqlServer.CreateSchemaObjects.Split(new string[] { "--go", "--GO" },
					StringSplitOptions.RemoveEmptyEntries);
				foreach (string statement in scripts)
					await mdb.ExecuteAsync(statement);
				log.Debug($"Schema SysCat was created in database {mdb.DbName}");
				log.Debug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
				string owner = await GetDefaultDbSchemaAsync(mdb);
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
				log.LogError($"CreateSchema error: {e.Message}");
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
					throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.SysCat.SysCatManager.getSchema.1", $"{areaName}.{objectName}"));
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
					throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.SysCat.SysCatManager.getSchema.2",
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
			await DropSchemaAsync(this.MdbContext, this.Logger, name.AreaName, name.ObjectName).ConfigureAwait(false);
			lock (objLock)
				_schemaCache.Remove(name.ToString());
		}
		private static async Task DropSchemaAsync(MdbContext mdb, ILogger logger, string areaName, string objectName)
		{
			var schema = await GetSchemaAsync(mdb, areaName, objectName);
			var schemaFromDb = await GetTableSchema(mdb, schema.DbObjectName.ToString());

			string[] sqlList;
			if (schemaFromDb == null)
				sqlList = new string[] { };
			else
				sqlList = await DropSchemaStatementsAsync(mdb, schemaFromDb);

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

		private static async Task<string[]> DropSchemaStatementsAsync(MdbContext mdb, JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			StringBuilder sb = new StringBuilder();
			await WriteDropStatementsAsync(mdb, sb, fromDbSchema);
			sql.Add(sb.ToString());
			return sql.ToArray();
		}

		private static async Task WriteDropStatementsAsync(MdbContext mdb, StringBuilder sb, JMXSchema fromDbSchema)
		{
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(detail_field_prefix)))
			{
				string[] names = att.FieldName.Split('_');
				var schemaFromDb = await GetTableSchema(mdb, new JMXObjectName(names[1], names[2]).ToString());
				if (schemaFromDb != null)
					await WriteDropStatementsAsync(mdb, sb, schemaFromDb);

			}
			string s = (await mdb.ExecuteAsync<string>(SqlServer.GetParentRelations,
				new MdbParameter("@table_name", fromDbSchema.DbObjectName.ToString()))) ?? "";
			if (!s.IsEmpty())
			{
				JArray a = JArray.Parse(s);
				foreach (var o in a)
				{
					string sch = (string)o["ParentObject"]["AreaName"];
					string tbl = (string)o["ParentObject"]["ObjectName"];
					if (!fromDbSchema.Attributes.Any(at =>
						at.FieldName == $"{detail_field_prefix}{sch}_{tbl}"))
						WriteDropParentRelationStatement(sb, o);
				}
			}
			foreach (var fk in fromDbSchema.ForeignKeys)
				WriteDropFKStatement(sb, fromDbSchema, fk);
			WriteDropTableStatement(sb, fromDbSchema);
		}

		private static void WriteDropParentRelationStatement(StringBuilder sb, JToken o)
		{
			sb.AppendFormat("alter table [{0}].[{1}] drop constraint [{2}]\n",
				(string)o["ParentObject"]["AreaName"],
				(string)o["ParentObject"]["ObjectName"],
				(string)o["KeyName"]);
		}

		private static void WriteDropFKStatement(StringBuilder sb, JMXSchema fromDbSchema, JMXForeignKey fk)
		{
			sb.AppendFormat("alter table [{0}].[{1}] drop constraint [{2}]\n",
				fromDbSchema.DbObjectName.AreaName,
				fromDbSchema.DbObjectName.ObjectName,
				fk.KeyName);
		}

		private static void WriteDropTableStatement(StringBuilder sb, JMXSchema fromDbSchema, string tmpTableName = null)
		{
			sb.AppendFormat("drop table [{0}].[{1}]\n",
				fromDbSchema.DbObjectName.AreaName,
				tmpTableName.IsEmpty() ? fromDbSchema.DbObjectName.ObjectName : tmpTableName);
		}

		#endregion Drop Schema

		#region Clear Catalog
		public async Task ClearCatalogAsync()
		{
			await DropDbSchemaAsync(this.MdbContext, this.Logger).ConfigureAwait(false);
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
		{
			return SaveSchemaAsync(schema).GetAwaiter().GetResult();
		}
		public override async Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			var mdb = this.MdbContext;
			schema = await NormalizeSchemaAsync(mdb, schema);
			int id = await mdb.ExecuteAsync<int>(SqlServer.AddSysSchemas,
				new MdbParameter("@ObjectSchema", schema.ToString()),
				new MdbParameter("@Version", schema_version));
			schema.ID = id;
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
				throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.1"));

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
					throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.4",
						att.FieldName));

				string typeMap = _typeMap[att.DataType];
				if (att.ServerDataType.IsEmpty() || typeMap.IndexOf(att.ServerDataType) == -1)
					att.ServerDataType = typeMap.Split(';')[0];

				if (att.DataSize.IsEmpty())
				{
					MdbTypeInfo ti2 = _typeInfo[att.ServerDataType];
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
						throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.3",
							member.FieldName, "primary key"));
				}
			}
			else if (schema.Attributes.Any(a => a.DataType == MdbType.@object))
			{
				//Requires PK for table with object attributes
				var pk = new JMXPrimaryKey
				{
					KeyName = $"PK_{schema.DbObjectName.AreaName}_{schema.DbObjectName.ObjectName}"
				};
				var att = schema.Attributes.FirstOrDefault(a => a.Identity.IsIdentity);
				if (att == null)
					att = schema.Attributes.FirstOrDefault(a => a.AttribName == "ID" || a.FieldName == "ID");
				if (att == null)
					att = schema.Attributes.FirstOrDefault(a => a.DefaultConstraint != null &&
						a.DefaultConstraint.Definition.ToUpper().IndexOf("NEXT VALUE FOR") > -1);
				if (att != null)
				{
					if (att.FieldName.IsEmpty())
						att.FieldName = att.AttribName;
					pk.AddKeyMember(att.FieldName);
					schema.PrimaryKey = pk;
				}
				else
					//A primary key is required for a table that includes columns of type 'object'
					throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.5",
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
						throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.3",
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
						throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.3", member.FieldName, $"foreign key '{fk.KeyName}'"));
				}
			}
		}

		private static async Task NormalizeDetailsAsync(MdbContext mdb, JMXSchema schema)
		{
			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
			{
				if (att.ObjectSchema == null)
					//For an object type attribute, you must specify a schema
					throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchemaProviderDB.normalize.2"));

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
		public override IEnumerable<string> GetChildObjects(string objectName)
		{
			if (_parentRelations.TryGetValue(objectName, out var childObjectList))
				return childObjectList;
			return new List<string>();
		}
		public override async Task<JMXSchema> SyncSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			var schema = await SyncSchemaAsync(this.MdbContext, this.Logger, name.AreaName, name.ObjectName).ConfigureAwait(false);
			lock (objLock)
				_schemaCache[schema.ObjectName] = schema;
			return schema;
		}
		public static async Task<JMXSchema> SyncSchemaAsync(MdbContext mdb, ILogger log,  string dbSchema, string objectName)
		{
			var schema = await GetSchemaInternalAsync(mdb, dbSchema, objectName, 0);
			//schema was syncronized
			if (schema.SyncState == 1)
				return schema;

			foreach (var o in GetDependences(schema))
			{
				await SyncSchemaAsync(mdb, log, o.AreaName, o.ObjectName);
			}

			var schemaFromDb = await GetTableSchema(mdb, schema.DbObjectName.ToString());
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
				sqlList = await CompareSchemasAsync(mdb, schema, prevSchema);
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
		private static string[] CreateNewStatements(JMXSchema schema)
		{
			List<string> sql = new List<string>();
			StringBuilder sb = new StringBuilder();
			WriteCreateNewTableStatements(sb, schema);
			sql.Add(sb.ToString());
			return sql.ToArray();
		}

		private static void WriteCreateNewTableStatements(StringBuilder sb, JMXSchema schema)
		{
			WriteCreateTableStatements(sb, schema);
			WriteCreatePKStatement(sb, schema);
			foreach (var att in schema.Attributes)
				WriteCreateConstraintStatement(sb, schema, att);
			foreach (var index in schema.Indexes)
				WriteCreateIndexStatement(sb, schema, index);
			foreach (var fk in schema.ForeignKeys)
				WriteCreateFKStatement(sb, schema, fk);
			WriteCreateDetailTableStatements(sb, schema);
		}

		private static void WriteCreateTableStatements(StringBuilder sb, JMXSchema schema)
		{
			sb.Append($"create table {schema.DbObjectName.ToString()}");
			sb.Append("(\n");
			int count = schema.Attributes.Count;
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				sb.Append($"[{att.FieldName}]\t{att.ServerDataType}");

				MdbTypeInfo ti = _typeInfo[att.ServerDataType];
				if (att.DataType == MdbType.@decimal && !ti.FixedSize && !att.DataSize.IsEmpty())
					sb.Append($"({att.DataSize.Precision},{att.DataSize.Scale})");
				else if (variable_lenght_data_types.IndexOf(att.ServerDataType) > -1)
					if (att.DataSize.Size == -1)
						sb.Append("(max)");
					else
						sb.Append($"({att.DataSize.Size})");

				if (att.Identity.IsIdentity)
					sb.Append($"\tidentity({att.Identity.Seed},{att.Identity.Increment})");
				else
					sb.Append($"\t{att.NullOption}");

				if (i != count - 1)
					sb.Append(",\n");

				att.ID = i + 1;
			}
			sb.Append(")\n");
		}

		private static void WriteCreateDetailTableStatements(StringBuilder sb, JMXSchema schema)
		{
			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
				WriteCreateNewTableStatements(sb, att.ObjectSchema);
		}

		private static void WriteCreatePKStatement(StringBuilder sb, JMXSchema schema)
		{
			var pk = schema.PrimaryKey;
			if (pk != null)
			{
				sb.Append($"alter table {schema.DbObjectName} add constraint [{pk.KeyName}] primary key (");
				int count = pk.KeyMembers.Count;
				for (int i = 0; i < count; i++)
				{
					var m = pk.KeyMembers[i];
					sb.Append($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
					if (i != count - 1)
						sb.Append(", ");
					else
						sb.Append(")\n");

				}
			}
		}

		private static void WriteCreateIndexStatement(StringBuilder sb, JMXSchema schema, JMXIndex index)
		{
			//CREATE UNIQUE NONCLUSTERED INDEX [AK1_SysSchemas] ON [SysCat].[SysSchemas] ([SysAreaID] ASC, [ObjectName] ASC)
			sb.Append("create " + (index.IsUnique ? "unique " : "") + (index.ClusteredOption == 1 ? "clustered " : "nonclustered ") +
				$"index [{index.IndexName}] " +
				$"on {schema.DbObjectName.ToString()} (");
			int count = index.KeyMembers.Count;
			for (int i = 0; i < count; i++)
			{
				var m = index.KeyMembers[i];
				sb.Append($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
				if (i != count - 1)
					sb.Append(", ");
				else
					sb.Append(")\n");
			}
		}

		private static void WriteCreateFKStatement(StringBuilder sb, JMXSchema schema, JMXForeignKey fk)
		{
			//Можно сделать сначала с NOCHECK затем CHECK
			//	ALTER TABLE[SysCat].[SysSchemas] WITH CHECK ADD CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]([SysAreaID]) REFERENCES[SysCat].[SysAreas] ([ID]) 
			//	ALTER TABLE[SysCat].[SysSchemas] CHECK CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]
			if (fk.CheckOption)
				// Enable for new added rows
				sb.Append($"alter table {schema.DbObjectName} with check  add constraint [{fk.KeyName}] foreign key (");
			else
				sb.Append($"alter table {schema.DbObjectName} with nocheck  add constraint [{fk.KeyName}] foreign key (");

			sb.Append(string.Join(", ", fk.KeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
			sb.Append(")");
			sb.Append($"references {fk.RefDbObjectName} (");
			sb.Append(string.Join(", ", fk.RefKeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
			sb.Append(")\n");
			if (fk.CheckOption)
				// check existing rows
				sb.Append($"alter table {schema.DbObjectName} check constraint [{fk.KeyName}]\n");
		}

		private static void WriteCreateConstraintStatement(StringBuilder sb, JMXSchema schema, JMXAttribute att)
		{
			if (!att.CheckConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} add constraint {att.CheckConstraint.ConstraintName} check({att.CheckConstraint.Definition})\n");
			if (!att.DefaultConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} add constraint {att.DefaultConstraint.ConstraintName} default({att.DefaultConstraint.Definition}) for [{att.FieldName}]\n");
		}

		private static void WriteCreateParentRelationStatement(StringBuilder sb, JMXSchema schema, JToken fk)
		{
			bool withCheck = (bool)fk["CheckOption"];
			string check = withCheck ? "check" : "nocheck";
			// Enable for new added rows
			sb.AppendFormat("alter table [{0}].[{1}] with {2} add constraint [{3}] foreign key (",
			(string)fk["ParentObject"]["AreaName"],
			(string)fk["ParentObject"]["ObjectName"],
			check, (string)fk["KeyName"]);


			sb.Append(string.Join(", ", fk["KeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			sb.Append(")");
			sb.Append($"references [{fk["RefObject"]["AreaName"]}].[{fk["RefObject"]["ObjectName"]}] (");
			sb.Append(string.Join(", ", fk["RefKeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			sb.Append(")\n");
			if (withCheck)
				// check existing rows
				sb.Append($"alter table [{fk["ParentObject"]["AreaName"]}].[{fk["ParentObject"]["ObjectName"]}] " +
					$"check constraint [{(string)fk["KeyName"]}]\n");

		}

		private static async Task<string[]> CompareSchemasAsync(MdbContext mdb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			StringBuilder sb = new StringBuilder();

			await CompareSchemasStatementsAsync(mdb, sb, schema, fromDbSchema);

			if (sb.Length > 1)
				sql.Add(sb.ToString());
			return sql.ToArray();
		}

		private static async Task CompareSchemasStatementsAsync(MdbContext mdb, StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			bool recreate = false;
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(detail_field_prefix)))
			{
				if (!schema.Attributes.Any(a => a.FieldName == att.FieldName))
				{
					string[] names = att.FieldName.Split('_');
					var schemaFromDb = await GetTableSchema(mdb, new JMXObjectName(names[1], names[2]).ToString());
					if (schemaFromDb != null)
						await WriteDropStatementsAsync(mdb, sb, schemaFromDb);
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
				var schemaFromDb = await GetTableSchema(mdb, att.ObjectSchema.DbObjectName.ToString());
				if (schemaFromDb != null)
					await CompareSchemasStatementsAsync(mdb, sb, att.ObjectSchema, schemaFromDb);
				else
					WriteCreateNewTableStatements(sb, att.ObjectSchema);

			}

		}

		private static bool CompareAttributes(StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			bool recreate = false;
			int count = schema.Attributes.Count;
			List<(JMXAttribute att, JMXAttribute att2, AttribCompareDiff diff)> l =
				new List<(JMXAttribute, JMXAttribute, AttribCompareDiff)>();
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				//var att2 = fromDbSchema.Attributes.FirstOrDefault(a => a.ID == att.ID);
				var att2 = fromDbSchema.Attributes.FirstOrDefault(a =>
					a.FieldName.Equals(att.FieldName, StringComparison.CurrentCultureIgnoreCase));
				//bool found = (att2 != null && att.FieldName.Equals(att2.FieldName, StringComparison.CurrentCultureIgnoreCase));
				bool found = (att2 != null);
				AttribCompareDiff diff = AttribCompareDiff.none;
				if (found)
				{
					if (!att.ServerDataType.Equals(att2.ServerDataType, StringComparison.CurrentCultureIgnoreCase))
						diff = AttribCompareDiff.dataTtype;
					if (att.Required != att2.Required)
						diff |= AttribCompareDiff.nullOptions;
					if (att.Identity.IsIdentity != att2.Identity.IsIdentity ||
						att.Identity.Seed != att2.Identity.Seed ||
						att.Identity.Increment != att2.Identity.Increment)
						diff |= AttribCompareDiff.identity;

					if (att.DataType != MdbType.@object &&
						!att.AttribName.Equals(att2.FieldName, StringComparison.CurrentCultureIgnoreCase))
						diff |= AttribCompareDiff.name;

					//Server DataTypes is equals
					if ((diff & AttribCompareDiff.dataTtype) != AttribCompareDiff.dataTtype)
					{
						MdbTypeInfo ti = _typeInfo[att.ServerDataType];
						if (!ti.FixedSize)
						{
							if (variable_lenght_data_types.IndexOf(att.ServerDataType) > -1 && att.DataSize.Size != att2.DataSize.Size)
								diff |= AttribCompareDiff.size;
							else if (ti.MdbType == MdbType.@decimal && (att.DataSize.Precision != att2.DataSize.Precision ||
								att.DataSize.Scale != att2.DataSize.Scale))
								diff |= AttribCompareDiff.size;
						}
					}
					if (!att.CheckConstraint.Definition.RemoveChar("[( )]".ToCharArray()).
						Equals(att2.CheckConstraint.Definition.RemoveChar("[( )]".ToCharArray()),
						StringComparison.CurrentCultureIgnoreCase))
						diff |= AttribCompareDiff.constraint;
					else if (!att.DefaultConstraint.Definition.RemoveChar("[( )]".ToCharArray()).
						Equals(att2.DefaultConstraint.Definition.RemoveChar("[( )]".ToCharArray()),
						StringComparison.CurrentCultureIgnoreCase))
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
					a.FieldName.Equals(att2.FieldName, StringComparison.InvariantCultureIgnoreCase));
				if (att == null)
					l.Add((att, att2, AttribCompareDiff.remove));
			}

			foreach (var (att, att2, diff) in l)
			{
				if ((diff & AttribCompareDiff.remove) == AttribCompareDiff.remove)
				{
					WriteDropColumnStatement(sb, schema, att2);
					continue;
				}
				if ((diff & AttribCompareDiff.constraint) == AttribCompareDiff.constraint)
				{
					WriteDropConstraintStatement(sb, fromDbSchema, att2);
					WriteCreateConstraintStatement(sb, schema, att);
				}
				if ((diff & AttribCompareDiff.dataTtype) == AttribCompareDiff.dataTtype ||
					(diff & AttribCompareDiff.size) == AttribCompareDiff.size ||
					(diff & AttribCompareDiff.nullOptions) == AttribCompareDiff.nullOptions)
					WriteAlterColumnStatement(sb, schema, att);
				else if ((diff & AttribCompareDiff.notFound) == AttribCompareDiff.notFound)
					WriteAlterColumnStatement(sb, schema, att, true);
				else if ((diff & AttribCompareDiff.name) == AttribCompareDiff.name)
				{
					att.FieldName = att.AttribName;
					WriteRenameColumnStatement(sb, schema, att2.FieldName, att.FieldName);
				}
				else if ((diff & AttribCompareDiff.identity) == AttribCompareDiff.identity)
					recreate = true;
			}
			return recreate;
		}

		private static bool ComparePK(StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
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
				WriteCreatePKStatement(sb, schema);
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

		private static void CompareIndexes(StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
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
					WriteDropIndexStatement(sb, fromDbSchema, i2);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					WriteCreateIndexStatement(sb, schema, i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					WriteDropIndexStatement(sb, fromDbSchema, i2);
					WriteCreateIndexStatement(sb, schema, i1);
				}
			}

		}

		private static void CompareFK(StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
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
					!k1.RefDbObjectName.ToString().Equals(k2.RefDbObjectName.ToString(), StringComparison.CurrentCultureIgnoreCase))
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
					WriteDropFKStatement(sb, fromDbSchema, i2);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					WriteCreateFKStatement(sb, schema, i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					WriteDropFKStatement(sb, fromDbSchema, i2);
					WriteCreateFKStatement(sb, schema, i1);
				}
			}
		}

		private static async Task RecreateSchemaAsync(MdbContext mdb, StringBuilder sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			int recCount = (await mdb.ExecuteAsync<int>($"select count(*) from {fromDbSchema.DbObjectName.ToString()}"));

			string s = (await mdb.ExecuteAsync<string>(SqlServer.GetParentRelations,
				new MdbParameter("@table_name", schema.DbObjectName.ToString()))) ?? "";
			JArray parentRelations = null;
			if (!s.IsEmpty())
			{
				parentRelations = JArray.Parse(s);
				foreach (var fk in parentRelations)
					WriteDropParentRelationStatement(sb, fk);
			}
			foreach (var fk in fromDbSchema.ForeignKeys)
				WriteDropFKStatement(sb, fromDbSchema, fk);

			string tmpTableName = fromDbSchema.DbObjectName.ObjectName + "_" + DateTime.Now.Subtract(vbo.Date()).Seconds.ToString();
			if (recCount > 0)
				WriteRenameTableStatement(sb, fromDbSchema, tmpTableName);
			else
				WriteDropTableStatement(sb, fromDbSchema);

			WriteCreateTableStatements(sb, schema);

			if (recCount > 0)
			{
				WriteInsertRowsStatement(sb, schema, tmpTableName);
				WriteDropTableStatement(sb, fromDbSchema, tmpTableName);
			}

			WriteCreatePKStatement(sb, schema);
			foreach (var att in schema.Attributes)
				WriteCreateConstraintStatement(sb, schema, att);
			foreach (var index in schema.Indexes)
				WriteCreateIndexStatement(sb, schema, index);
			foreach (var fk in schema.ForeignKeys)
				WriteCreateFKStatement(sb, schema, fk);


			if (parentRelations != null)
			{
				foreach (var fk in parentRelations)
					WriteCreateParentRelationStatement(sb, schema, fk);
			}
		}

		private static void WriteRenameTableStatement(StringBuilder sb, JMXSchema schema, string newName)
		{
			//EXEC sp_rename 'Sales.SalesTerritory', 'SalesTerr'; 
			sb.Append($"EXEC sp_rename '{schema.DbObjectName}', '{newName}'\n");
		}

		private static void WriteInsertRowsStatement(StringBuilder sb, JMXSchema schema, string tmpTableName)
		{
			/// [dbo].[Customers]
			//		([Name])

			//select
			//	Name
			//From Bank.dbo.Clients where ClType = 0;
			sb.Append($"insert into {schema.DbObjectName} (\n");
			int count = schema.Attributes.Count;
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				if (!att.Identity.IsIdentity)
				{
					sb.Append($"[{att.FieldName}]");
					if (i != count - 1)
						sb.Append(",\n");
				}
				att.ID = i + 1;
			}
			sb.Append(")\nselect\n");
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				if (!att.Identity.IsIdentity)
				{
					sb.Append($"[{att.FieldName}]");
					if (i != count - 1)
						sb.Append(",\n");
				}
				att.ID = i + 1;
			}
			sb.Append($"from [{schema.DbObjectName.AreaName}].[{tmpTableName}]");
		}

		/// <summary>
		/// ALTER TABLE only allows columns to be added that can contain nulls, 
		/// or have a DEFAULT definition specified, 
		/// or the column being added is an identity or timestamp column, 
		/// or alternatively if none of the previous conditions are satisfied the table must be empty
		/// to allow addition of this column. 
		/// </summary>
		private static void WriteAlterColumnStatement(StringBuilder sb, JMXSchema schema, JMXAttribute att, bool addNew = false)
		{
			string action = addNew ? "add" : "alter column";
			sb.Append($"alter table [{schema.DbObjectName.AreaName}].[{schema.DbObjectName.ObjectName}] {action} [{att.FieldName}]\t{att.ServerDataType}");

			MdbTypeInfo ti = _typeInfo[att.ServerDataType];
			if (att.DataType == MdbType.@decimal && !ti.FixedSize && !att.DataSize.IsEmpty())
				sb.Append($"({att.DataSize.Precision},{att.DataSize.Scale})");
			else if (variable_lenght_data_types.IndexOf(att.ServerDataType) > -1)
				if (att.DataSize.Size == -1)
					sb.Append("(max)");
				else
					sb.Append($"({att.DataSize.Size})");
			sb.Append($"\t{att.NullOption}\n");
		}

		private static void WriteRenameColumnStatement(StringBuilder sb, JMXSchema schema, string oldName, string newName)
		{
			sb.Append($"exec sp_rename '{schema.DbObjectName}.{oldName}', '{newName}', 'COLUMN'\n");
		}

		private static void WriteDropColumnStatement(StringBuilder sb, JMXSchema schema, JMXAttribute att)
		{
			sb.Append($"alter table [{schema.DbObjectName.AreaName}].[{schema.DbObjectName.ObjectName}] drop column [{att.FieldName}]\n");
		}

		private static void WriteDropIndexStatement(StringBuilder sb, JMXSchema schema, JMXIndex index)
		{
			sb.Append($"drop index {index.IndexName} on {schema.DbObjectName}\n");
		}

		private static void WriteDropPKStatement(StringBuilder sb, JMXSchema schema)
		{
			sb.Append($"alter table {schema.DbObjectName} drop constraint {schema.PrimaryKey.KeyName}\n");
		}

		private static void WriteDropConstraintStatement(StringBuilder sb, JMXSchema schema, JMXAttribute att)
		{
			if (!att.DefaultConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} drop constraint {att.DefaultConstraint.ConstraintName}\n");
			if (!att.CheckConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} drop constraint {att.CheckConstraint.ConstraintName}\n");
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
		private static async Task<JMXSchema> GetTableSchema(MdbContext mdb, string fullTableName)
		{
			if ((await GetSqlVersion(mdb)).ToIntOrDefault() > 12)
			{
				string s = await mdb.ExecuteAsync<string>(SqlServer.GetTableSchema,
					new MdbParameter("@table_name", fullTableName));
				if (s != null)
					return JMXSchema.Parse(s);
			}
			else
			{
				string s = await mdb.ExecuteAsync<string>(SqlServer.GetTableSchema_xml,
					new MdbParameter("@table_name", fullTableName));
				if (s != null)
					return JMXSchema.ParseXml(s);
			}
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
					_sqlVersion = _sqlVersion.Split('.')[0];
			}
			return _sqlVersion;
		}
		private static JMXObjectName[] GetDependences(JMXSchema schema)=>
			schema.ForeignKeys.Select(fk => fk.RefDbObjectName).ToArray();
		#endregion Utils
	}
}
