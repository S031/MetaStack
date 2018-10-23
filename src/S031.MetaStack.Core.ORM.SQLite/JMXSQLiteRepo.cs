using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;

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

		private static readonly Dictionary<string, MdbTypeInfo> _typeInfo = new Dictionary<string, MdbTypeInfo>(StringComparer.CurrentCultureIgnoreCase)
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

		private static string _defaultDbSchema = string.Empty;

		public JMXSQLiteRepo(MdbContext mdbContext, ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(JMXSQLiteFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSQLiteFactory.ProviderInvariantName} provider.");
			Logger = logger;
			//Костыль need 	
			//TestSysCatAsync()
			//	.ConfigureAwait(false)
			//	.GetAwaiter()
			//	.GetResult();
		}

		public override IDictionary<MdbType, string> GetTypeMap() => _typeMap;

		public override IDictionary<string, MdbTypeInfo> GetServerTypeMap() => _typeInfo;

		public override string[] GetVariableLenghtDataTypes() => variable_lenght_data_types.Split(';');

		public override JMXSchema GetSchema(string objectName) => GetSchemaAsync(objectName).GetAwaiter().GetResult();

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
				var result = await this.MdbContext.ExecuteAsync<string>(SQLite.TestSchema);
				if (result.IsEmpty())
				{
					await CreateDbSchemaAsync(this.MdbContext, this.Logger);
					_defaultDbSchema = await GetDefaultDbSchemaAsync(this.MdbContext);
					Interlocked.Increment(ref _counter);
				}
			}
		}

		private void TestSysCat()
		{
			if (_counter == 0)
			{
				var result = this.MdbContext.Execute<string>(SQLite.TestSchema);
				if (result.IsEmpty())
				{
					CreateDbSchemaAsync(this.MdbContext, this.Logger).GetAwaiter().GetResult();
					_defaultDbSchema = GetDefaultDbSchemaAsync(this.MdbContext).GetAwaiter().GetResult();
					Interlocked.Increment(ref _counter);
				}
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
				log.Debug($"Schema SysCat was created in database {mdb.DbName}");
				log.Debug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
				string owner = await GetDefaultDbSchemaAsync(mdb);
				int id = await mdb.ExecuteAsync<int>(SQLite.AddSysAreas,
					new MdbParameter("@SchemaName", owner),
					new MdbParameter("@SchemaOwner", owner),
					new MdbParameter("@SchemaVersion", schema_version));
				id = await mdb.ExecuteAsync<int>(SQLite.AddSysAreas,
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
	}
}