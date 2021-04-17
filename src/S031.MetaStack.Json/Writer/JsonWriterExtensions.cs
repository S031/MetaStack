using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public static class JsonWriterExtensions
	{
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, object value)
		{
			if (value != null)
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(JsonHelper.ToJsonValue(value));
			}
			return writer;
		}
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(value);
			}
			return writer;
		}
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, int value)
		{
			if (value != 0)
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(value);
			}
			return writer;
		}
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, double value)
		{
			if (value != 0)
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(value);
			}
			return writer;
		}
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, DateTime value)
		{
			if (value != default)
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(value);
			}
			return writer;
		}
		public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, bool value)
		{
			if (value)
			{
				writer.WritePropertyName(propertyName)
					.WriteValue(value);
			}
			return writer;
		}
	}
}
