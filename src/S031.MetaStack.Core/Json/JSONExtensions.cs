using System;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using Newtonsoft.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.Json
#else
namespace S031.MetaStack.WinForms.Json
#endif
{
	public static class JSONExtensions
    {
        public static bool ToBoolOrDefault(this JToken jsonValue)
        {
			if (jsonValue == null)
				return false;

			JTokenType type = jsonValue.Type;
			if (type == JTokenType.String)
			{
				string source = ((string)jsonValue).ToLower();
				if (source == "1" || source == "true")
					return true;
				else if (source == "0" || source == "false")
					return false;
				else if (bool.TryParse(source, out bool val))
					return val;
				else
					return false;
			}
			else
				return !jsonValue.IsNullOrEmpty();
        }
		public static long ToIntOrDefault(this JToken jsonValue)
		{
			if (jsonValue == null)
				return 0;

			JTokenType type = jsonValue.Type;
			if (type == JTokenType.Boolean)
				return (bool)jsonValue ? 1 : 0;
			else if (type == JTokenType.Integer)
				return (long)jsonValue;
			else if (type == JTokenType.Float)
				return (long)jsonValue;
			else if (type == JTokenType.String)
				return long.TryParse((string)jsonValue, out long result) ? result : 0;
			else
				return 0;

		}
		public static decimal ToDecimalOrDefault(this JToken jsonValue)
		{
			if (jsonValue == null)
				return 0;

			JTokenType type = jsonValue.Type;
			if (type == JTokenType.Boolean)
				return (bool)jsonValue ? 1 : 0;
			else if (type == JTokenType.Integer)
				return (decimal)jsonValue;
			else if (type == JTokenType.Float)
				return (decimal)jsonValue;
			else if (type == JTokenType.String)
				return decimal.TryParse((string)jsonValue, out decimal result) ? result : 0;
			else
				return 0;

		}
		public static DateTime ToDateOrDefault(this JToken jsonValue)
		{
			string str;
			if (jsonValue == null)
				return DateTime.MinValue;

			JTokenType type = jsonValue.Type;
			if (type == JTokenType.Integer || 
				type == JTokenType.Float)
				str = ((decimal)jsonValue).ToString();
			else if (type == JTokenType.String)
				str = (string)jsonValue;
			else
				return DateTime.MinValue;

			if (string.IsNullOrEmpty(str) || str == "0" || str.Left(4) == "0:00" || str.Left(5) == "00:00")
				return DateTime.MinValue;
			else if (DateTime.TryParse(str, out DateTime val))
				return val;
			return DateTime.MinValue;
		}
		public static bool IsNullOrEmpty(this JToken jsonValue)
		{
			return (jsonValue == null) ||
				   (jsonValue.Type == JTokenType.Null) ||
				   (jsonValue.Type == JTokenType.Boolean && !(bool)jsonValue) ||
				   (jsonValue.Type == JTokenType.Integer && (long)jsonValue == 0) ||
				   (jsonValue.Type == JTokenType.Float && (float)jsonValue == 0) ||
				   (jsonValue.Type == JTokenType.Date && (DateTime)jsonValue == DateTime.MinValue) ||
				   (jsonValue.Type == JTokenType.Array && !jsonValue.HasValues) ||
				   (jsonValue.Type == JTokenType.Object && !jsonValue.HasValues) ||
				   (jsonValue.Type == JTokenType.String && jsonValue.ToString() == String.Empty);
		}
		public static object GetValue(this JToken jsonValue)
		{
			if (jsonValue.Type == JTokenType.Boolean)
				return (bool)jsonValue;
			else if (jsonValue.Type == JTokenType.Integer)
				return (long)jsonValue;
			else if (jsonValue.Type == JTokenType.Float)
				return (float)jsonValue;
			else if (jsonValue.Type == JTokenType.String)
				return (string)jsonValue;
			else if (jsonValue.Type == JTokenType.Date)
				return (DateTime)jsonValue;
			else if (jsonValue.Type == JTokenType.Null)
				return null;
			else if (jsonValue.Type == JTokenType.Object)
				return JObject.FromObject(jsonValue);
			else
				return jsonValue.ToString();
		}
		public static object DeserializeObject(string json)
		{
			return JsonConvert.DeserializeObject(json,
				new Newtonsoft.Json.JsonSerializerSettings()
				{
					MissingMemberHandling = MissingMemberHandling.Ignore,
					Error = (sender, args) => args.ErrorContext.Handled = true
				});

		}
		public static T DeserializeObject<T>(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(json,
					new Newtonsoft.Json.JsonSerializerSettings()
					{
						MissingMemberHandling = MissingMemberHandling.Ignore,
						Error = (sender, args) => args.ErrorContext.Handled = true
					});
			}
			catch
			{
				return default;
			}
		}
		public static string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value,
				new Newtonsoft.Json.JsonSerializerSettings()
				{
					Error = (sender, args) => args.ErrorContext.Handled = true
				});

		}
	}
}
