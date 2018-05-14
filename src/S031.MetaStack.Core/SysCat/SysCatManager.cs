using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.SysCat
{
	public partial class SysCatManager
	{
		/// <summary>
		/// ??? Надо разобраться нужно ли использовать db_name
		/// </summary>
		const string db_name = "MetaStack";
		const string schema_version = "0.0.1";

		private static string _defaultDbSchema= string.Empty;
		private static readonly Dictionary<string, JObject> _schemaCache = new Dictionary<string, JObject>();

		private MdbContext _mdb;
		private ILogger _log;
		private object objLock = new object();
		public SysCatManager(MdbContext context)
		{
			context.NullTest(nameof(context));
			_mdb = context;
			_log = _mdb.Logger;
			if (_defaultDbSchema.IsEmpty())
				_defaultDbSchema = _mdb.Execute<string>(SQLCat.GetCatalog(_mdb.ProviderName)["GetDefaultSchema"]);
		}

		public async Task CreateDbSchemaAsync(string dbName = db_name)
		{
			var s = SQLCat.GetCatalog(_mdb.ProviderName);
			var test = await _mdb.ExecuteAsync<string>(s["TestSchema"]);
			if (test.IsEmpty())
			{
				try
				{
					await _mdb.BeginTransactionAsync();
					foreach (string statement in s["CreateSchemaObjects"].Split("--go", StringSplitOptions.RemoveEmptyEntries))
						await _mdb.ExecuteAsync(statement);
					_log.LogDebug($"Schema SysCat was created in database {_mdb.DbName}");
					_log.LogDebug("Schema tables SysCat.SysAreas and SysCat.SysSchemas was created in SysCat Schema");
					string owner = _defaultDbSchema; //await _mdb.ExecuteAsync<string>(s["GetCurrentSchema"]);
					int id = await _mdb.ExecuteAsync<int>(s["AddSysAreas"],
						new MdbParameter("@SchemaName", owner),
						new MdbParameter("@SchemaOwner", owner),
						new MdbParameter("@SchemaVersion", schema_version));
					await _mdb.CommitAsync();
				}
				catch (Exception e)
				{
					_mdb.RollBack();
					_log.LogError($"CreateSchema error: {e.Message}");
					throw;
				}
			}
			else
				_log.LogDebug("Schema SysCat already exists in current database");
		}
		public async Task DropDbSchemaAsync(string dbName = db_name)
		{
			var s = SQLCat.GetCatalog(_mdb.ProviderName);
			var test = await _mdb.ExecuteAsync<string>(s["TestSchema"]);
			if (!test.IsEmpty())
			{
				try
				{
					await _mdb.ExecuteAsync(s["DropSchema"]);
					_log.LogDebug($"Schema SysCat was deleted from database {_mdb.DbName}");
				}
				catch (Exception e)
				{
					_log.LogError($"Delete Schema error: {e.Message}");
					throw;
				}
			}
			else
				_log.LogDebug($"Schema SysCat not exists in database  {_mdb.DbName}");
		}

		async Task<(string DbSchema, string ObjectName, JObject ObjectSchema)> getSchemaAsync(string objectName)
		{
			string dbSchema = _defaultDbSchema;
			int pos = objectName.IndexOf('.');
			string key;
			if (pos > -1)
			{
				key = objectName;
				dbSchema = objectName.Left(pos - 1);
				objectName = objectName.Substring(pos + 1);
			}
			else
				key = $"{dbSchema}.{objectName}";

			if (!_schemaCache.TryGetValue(key, out var objectSchema))
			{
				string sch = await _mdb.ExecuteAsync<string>($"select ObjectSchema from SysCat.V_SysSchemas where SchemaName = '{dbSchema}' and " +
					$"ObjectName = '{objectName}'");
				if (sch.IsEmpty())
					//object schema not found in database
					throw new KeyNotFoundException(Translater.GetTranslate("S031.MetaStack.Core.SysCat.SysCatManager.getSchema.1", key));
				objectSchema = JObject.Parse(sch);
				lock (objLock)
					_schemaCache.Add(key, objectSchema);
			}
			return (dbSchema, objectName, objectSchema);
		}
	}
}
