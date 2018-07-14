using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using S031.MetaStack.Common;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	internal static class JsonWriterExtensions
    {
		public static void WriteProperty(this JsonWriter writer, string propertyName, object value)
		{
			if (!vbo.IsEmpty(value))
			{
				writer.WritePropertyName(propertyName);
				writer.WriteValue(value);
			}
		}
		public static void WriteProperty(this JsonWriter writer, string propertyName, string value)
		{
			if (!value.IsEmpty())
			{
				writer.WritePropertyName(propertyName);
				writer.WriteValue(value);
			}
		}
		public static void WriteProperty(this JsonWriter writer, string propertyName, int value)
		{
			if (value != 0)
			{
				writer.WritePropertyName(propertyName);
				writer.WriteValue(value);
			}
		}
		public static void WriteProperty(this JsonWriter writer, string propertyName, DateTime value)
		{
			if (value != default(DateTime))
			{
				writer.WritePropertyName(propertyName);
				writer.WriteValue(value);
			}
		}
		public static void WriteProperty(this JsonWriter writer, string propertyName, bool value)
		{
			if (value)
			{
				writer.WritePropertyName(propertyName);
				writer.WriteValue(value);
			}
		}
    }
}
