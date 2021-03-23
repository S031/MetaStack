using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Caching;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		//private readonly ILogger _logger;
		private readonly IMdbContextFactory _mdbFactory;
		private static readonly SettingsCache _cache = SettingsCache.Instance;

		public VocabularySettingsProvider(IServiceProvider services)
		{
			//_logger =  services.GetRequiredService<ILogger>();
			_mdbFactory = services.GetRequiredService<IMdbContextFactory>();
		}

		public async Task<string> GetSettings(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("Invalid argument for get settings", nameof(path));
			
			string[] items = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
			bool needJson = items.Length > 1;

			JsonWriter w = new JsonWriter(Formatting.None);
			JsonValue settings = null;
			
			if (!_cache.TryGetValue(items[0], out string s))
			{
				using (MdbContext mdb = _mdbFactory.GetContext(Properties.Strings.DefaultConnection))
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
							string source = (string)dr.GetValue(i);
							if (needJson && !JsonValue.TryParse(source, out settings))
								throw new KeyNotFoundException($"No found setting with path = '{path}' Or ValueString is not json format");

							w.WritePropertyName("Settings");
							if (needJson || JsonValue.Test(source))
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
			else if (needJson)
				settings = new JsonReader(s).Read()["Settings"];

			if (!needJson)
				return s;
			else if (settings == null)
				throw new InvalidCastException($"ValueString has not valid json format");

			var result = settings[items[1]];
			for (int i = 2; i < items.Length; i++)
				result = result[items[i]];
			return result.ToString();
		}
	}
}
