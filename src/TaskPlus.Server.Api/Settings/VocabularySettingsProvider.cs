using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
			where [Identifier] = @identifier";

		private ILogger _logger;
		private IMdbContextFactory _mdbFactory;
		private static readonly SettingsCache _cache = SettingsCache.Instance;

		public VocabularySettingsProvider(IServiceProvider services)
		{
			_logger =  services.GetRequiredService<ILogger>();
			_mdbFactory = services.GetRequiredService<IMdbContextFactory>();
		}

		public async Task<string> GetSettings(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("Invalid argument for get settings", nameof(path));
			
			string[] items = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
			JsonWriter w = new JsonWriter(Formatting.None);
			
			if (!_cache.TryGetValue(items[0], out string s))
			{
				using (MdbContext mdb = _mdbFactory.GetContext("DefaultConnection"))
				using (var dr = await mdb.GetReaderAsync(_sql_get_settings,
					new MdbParameter("@identifier", items[0])))
				{
					if (!dr.Read())
						throw new KeyNotFoundException($"No found setting with Identifier = '{items[0]}'");
					w.WriteStartObject();
					for (int i = 0; i < dr.FieldCount; i++)
					{
						string name = dr.GetName(i);
						if (name == "ValueString")
						{
							w.WritePropertyName("Settings");
							string source = ((string)dr.GetValue(i)).Trim();
							if (source.StartsWith('{'))
								w.WriteRaw(source);
							else
								w.WriteValue(source);
						}
						else
							w.WriteProperty(name, dr.GetValue(i));
					}
					w.WriteEndObject();
					s = w.ToString();
					_cache.TryAdd(items[0], s);
				}
			}
			if (items.Length == 1)
				return s;
			_logger.LogDebug(s);
			JsonValue result = new JsonReader(s).Read()["Settings"];
			if (result == null)
				throw new KeyNotFoundException($"No found setting for path '{path}'");

			for (int n = 1; n < items.Length; n++)
				/// !!! add exception raise
				result = result[items[n]];
			return result;
		}
	}
}
