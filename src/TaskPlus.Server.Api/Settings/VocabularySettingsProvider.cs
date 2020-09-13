using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Caching;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskPlus.Server.Data;

namespace TaskPlus.Server.Api.Settings
{
	public class VocabularySettingsProvider : ISettingsProvider<VocabularySettingsProvider>
	{
		private const string _sql_get_settings = @"
			SELECT [SettingsID]
				  ,IsNull([Title], '') as [Title]
				  ,[Identifier]
				  ,IsNull([Description], '') as [Description]
				  ,IsNull([XMLDescription], '<XMLDescription />') as [XMLDescription]
				  ,IsNull([ValueDouble], 0) as [ValueDouble]
				  ,IsNull([ValueMoney], 0) as [ValueMoney]
				  ,IsNull([ValueDate], '1900101') as [ValueDate]
				  ,isnull([ValueString], '') as [ValueString]
				  ,[ValueBool]
			FROM [BU_Earth].[dbo].[Vocabulary_SettingsFactoring]
			where [Identifier] = '{@identifier}'";

		private IMdbContextFactory _mdbFactory;
		private static readonly SettingsCache _cache = SettingsCache.Instance;

		public VocabularySettingsProvider(IServiceProvider services)
		{
			_mdbFactory = services.GetRequiredService<IMdbContextFactory>();
		}

		public async Task<JsonValue> GetSettings(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("Invalid argument for get settings", nameof(path));
			
			string[] items = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
			JsonObject j;
			if (!_cache.TryGetValue(items[0], out JsonValue jv))
			{
				j = new JsonObject();
				using (MdbContext mdb = _mdbFactory.GetContext("DefaultConnection"))
				using (var dr = await mdb.GetReaderAsync(_sql_get_settings,
					"@identifier", items[0]))
				{
					if (!dr.Read())
						throw new KeyNotFoundException($"No found setting with Identifier = '{items[0]}'");
					for (int i = 0; i < dr.FieldCount; i++)
					{
						string name = dr.GetName(i);
						if (name == "ValueString")
						{
							string source = ((string)dr.GetValue(i)).Trim();
							if (source.StartsWith('{'))
								j["Settings"] = (JsonObject)new JsonReader(source).Read();
							else
								j["Settings"] = null;
						}
						else
							j.Add(dr.GetName(i), new JsonValue(dr.GetValue(i)));
					}
					_cache.TryAdd(items[0], j);
				}
			}
			else
				j = (JsonObject)jv;

			if (items.Length == 1)
				return j;

			JsonValue result = j["Settings"];
			if (result == null)
				throw new KeyNotFoundException($"No found setting for path '{path}'");

			for (int n = 1; n < items.Length; n++)
				/// !!! add exception raise
				result = result[items[n]];
			return result;
		}
	}
}
