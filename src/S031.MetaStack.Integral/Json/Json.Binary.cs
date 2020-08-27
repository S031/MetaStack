using S031.MetaStack.Buffers;
using S031.MetaStack.Json;
using System;
using System.Runtime.CompilerServices;
using JsonPair = System.Collections.Generic.KeyValuePair<string, S031.MetaStack.Json.JsonValue>;

namespace S031.MetaStack.Json
{
	public static partial class JsonSerializer
	{
		public static byte[] ToBinary(this JsonObject jsonObject)
		{
			BinaryDataWriter bw = new BinaryDataWriter(2048);
			WriteRaw(jsonObject, ref bw);
			return bw.GetBytes();
		}

		public static byte[] ToBinary(this JsonArray jsonArray)
		{
			BinaryDataWriter bw = new BinaryDataWriter(2048);
			WriteArrayRaw(jsonArray, ref bw);
			return bw.GetBytes();
		}

		private static void WriteRaw(JsonObject jsonObject, ref BinaryDataWriter writer)
		{
			writer.WriteMapHeader(jsonObject.Count);
			foreach (JsonPair pair in jsonObject)
			{
				WritePropertyName(pair.Key, ref writer);
				WriteValue(pair.Value, ref writer);
			}
		}

		/// <summary>
		/// Property name must be a ascii string
		/// writed as one byte for character
		/// </summary>
		/// <param name="name"></param>
		/// <param name="writer"></param>
		private static unsafe void WritePropertyName(string name, ref BinaryDataWriter writer)
		{
			fixed (char* source = name)
				writer.Write(source, name.Length);
		}

		private static void WriteArrayRaw(JsonArray jsonArray, ref BinaryDataWriter writer)
		{
			writer.WriteArrayHeader(jsonArray.Count);
			foreach (JsonValue v in jsonArray)
			{
				if (v == null)
					writer.WriteNull();
				else
					WriteValue(v, ref writer);
			}
		}

		private static void WriteValue(JsonValue value, ref BinaryDataWriter writer)
		{
			switch (value.JsonType)
			{
				case JsonType.Boolean:
					writer.Write((bool)value);
					break;
				case JsonType.Integer:
					writer.Write(Convert.ToInt64(value.GetValue()));
					break;
				case JsonType.Float:
					writer.Write((double)value);
					break;
				case JsonType.Date:
					writer.Write((DateTime)value);
					break;
				case JsonType.String:
					writer.Write((string)value);
					break;
				case JsonType.Bytes:
					writer.Write((byte[])value.GetValue());
					break;
				case JsonType.Object:
					JsonObject o = (value as JsonObject);
					WriteRaw(o, ref writer);
					break;
				case JsonType.Array:
					JsonArray a = (value as JsonArray);
					WriteArrayRaw(a, ref writer);
					break;
				case JsonType.Guid:
					writer.Write((Guid)value);
					break;
				case JsonType.TimeSpan:
					writer.Write((TimeSpan)value);
					break;
				case JsonType.Null:
				case JsonType.None:
					writer.WriteNull();
					break;
				default:
					//writer.WriteRaw(MessagePackSerializer.Typeless.Serialize(value.GetValue()));
					break;
			}
		}

		/// <summary>
		/// Usage:
		/// <code
		///		var j = new JsonObject().FromBinary(byteArray)
		/// />
		/// </summary>
		/// <param name="jsonObject"><see cref="JsonObject"/></param>
		/// <param name="source"><see cref="byte[]"/></param>
		/// <returns></returns>
		public static JsonObject FromBinary(this JsonObject jsonObject, byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var r = new BinaryDataReader((BinaryDataBuffer)source);

			ExportedDataTypes t = r.ReadNext();
			if (t == ExportedDataTypes.@object)
				ReadRaw(jsonObject, ref r);
			else
				throw new FormatException("Invalid format for source data. Start type must be object.");
			return jsonObject;
		}

		/// <summary>
		/// Usage
		/// <code
		///		var j = new JsonArray().FromBinary(byteArray)
		/// />
		/// </summary>
		/// <param name="jsonArray"><see cref="JsonArray"/></param>
		/// <param name="source"><see cref="byte[]"/></param>
		/// <returns></returns>
		public static JsonArray FromBinary(this JsonArray jsonArray, byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var r = new BinaryDataReader((BinaryDataBuffer)source);

			ExportedDataTypes t = r.ReadNext();
			if (t == ExportedDataTypes.@object)
				ReadArrayRaw(jsonArray, ref r);
			else
				throw new FormatException("Invalid format for source data. Start type must be array.");
			return jsonArray;
		}

		public static JsonValue FromBinary(this JsonValue jsonValue, byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var r = new BinaryDataReader((BinaryDataBuffer)source);

			ExportedDataTypes t = r.ReadNext();
			if (t == ExportedDataTypes.@object)
				ReadRaw((JsonObject)jsonValue, ref r);
			else if (t == ExportedDataTypes.array)
				ReadArrayRaw((JsonArray)jsonValue, ref r);
			else
				throw new FormatException("Invalid format for source data. Start type must be object or array.");
			return jsonValue;

		}

		private static void ReadRaw(JsonObject jsonObject, ref BinaryDataReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var t = reader.ReadNext();
				if (t != ExportedDataTypes.asciiString)
					throw new FormatException("Expected property name string");
				
				string propertyName = reader.ReadAsciiString();
				if(string.IsNullOrEmpty(propertyName))
					throw new FormatException("Property name string must have a value");

				jsonObject[propertyName] = ReadValue(ref reader);
			}
		}

		private static void ReadArrayRaw(JsonArray jsonArray, ref BinaryDataReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
				jsonArray.Add(ReadValue(ref reader));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static JsonValue ReadValue(ref BinaryDataReader reader)
		{
			var t = reader.ReadNext();
			switch (t)
			{
				case ExportedDataTypes.@int:
					return reader.ReadInt32();
				case ExportedDataTypes.asciiString:
					return reader.ReadAsciiString();
				case ExportedDataTypes.@string:
					return reader.ReadString();
				case ExportedDataTypes.utf8String:
					return reader.ReadUtf8String();
				case ExportedDataTypes.@bool:
					return reader.ReadBool();
				case ExportedDataTypes.@byte:
					return reader.ReadByte();
				case ExportedDataTypes.@short:
					return reader.ReadInt16();
				case ExportedDataTypes.@ushort:
					return reader.ReadUInt16();
				case ExportedDataTypes.@uint:
					return reader.ReadUInt32();
				case ExportedDataTypes.@long:
					return reader.ReadInt64();
				case ExportedDataTypes.@ulong:
					return reader.ReadUInt64();
				case ExportedDataTypes.@float:
					return reader.ReadSingle();
				case ExportedDataTypes.@double:
					return reader.ReadDouble();
				case ExportedDataTypes.@decimal:
					return reader.ReadDecimal();
				case ExportedDataTypes.dateTime:
					return reader.ReadDate();
				case ExportedDataTypes.@guid:
					return reader.ReadGuid();
				case ExportedDataTypes.@null:
					return null;
				case ExportedDataTypes.@object:
					JsonObject j = new JsonObject();
					ReadRaw(j, ref reader);
					return j;
				case ExportedDataTypes.@array:
					JsonArray a = new JsonArray();
					ReadArrayRaw(a, ref reader);
					return a;
				case ExportedDataTypes.byteArray:
				default:
					throw new FormatException($"Not supported ExportedDataType {t}");
			}
		}
	}
}
