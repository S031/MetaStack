using S031.MetaStack.Buffers;
using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Data
{
	public sealed partial class DataPackage : IDataReader
	{
		public override string ToString() => ToString(TsExportFormat.JSON);

		public string ToString(TsExportFormat fmt)
		{
			if (fmt == TsExportFormat.JSON)
				return SaveJSON();
			else
				throw new NotImplementedException();
		}

		private string SaveJSON()
		{
			JsonWriter writer = new JsonWriter(MetaStack.Json.Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}

		private void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("HeaderSize", _headerSpaceSize);

			#region Heders
			writer.WritePropertyName("Headers");
			writer.WriteStartObject();
			foreach (var kvp in _headers)
				writer.WriteProperty(kvp.Key, kvp.Value);
			writer.WriteEndObject();
			#endregion Heders

			#region Columns
			writer.WritePropertyName("Columns");
			writer.WriteStartArray();
			for (int i = 0; i < _indexes.Length; i++)
			{
				ColumnInfo ci = _colInfo[i];
				writer.WriteStartObject();
				writer.WritePropertyName("Name");
				writer.WriteValue(_indexes[i]);
				writer.WritePropertyName("Type");
				writer.WriteValue(MdbTypeMap.GetTypeInfo(ci.DataType).Name);
				writer.WritePropertyName("Size");
				writer.WriteValue(ci.ColumnSize);
				writer.WritePropertyName("AllowNull");
				writer.WriteValue(ci.AllowDBNull);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
			#endregion Columns

			#region Data
			writer.WritePropertyName("Rows");
			GoDataTop();
			writer.WriteStartArray();
			for (; Read();)
				WriteRowJson(writer);
			writer.WriteEndArray();
			#endregion Data

			writer.WriteEndObject();
		}

		private void WriteRowJson(JsonWriter writer)
		{
			writer.WriteStartObject();
			for (int i = 0; i < _colCount; i++)
			{
				string colName = _indexes[i];
				writer.WriteProperty(colName, _dataRow[i]);
			}
			writer.WriteEndObject();
		}

		public string GetRowJSON()
		{
			JsonWriter writer = new JsonWriter(MetaStack.Json.Formatting.None);
			WriteRowJson(writer);
			return writer.ToString();
		}

		public static DataPackage Parse(int headerSpaceSize, string jsonString)
		{
			JsonObject j = (JsonObject)new JsonReader(jsonString).Read();
			DataPackage ts = new DataPackage(headerSpaceSize,
				j.Select(kvp => kvp.Key).ToArray<string>(),
				j.Select(kvp => kvp.Value?.GetValue()).ToArray<object>());
			//ts.Update();
			ts.GoDataTop();
			return ts;
		}

		public static DataPackage Parse(string jsonString)
		{
			JsonObject j = (JsonObject)new JsonReader(jsonString).Read();
			int headerSpaceSize = (int)j["HeaderSize"];

			JsonArray columns = (j["Columns"] as JsonArray);
			JsonArray rows = (j["Rows"] as JsonArray);
			string[] cis = columns.Select(
				c => $"{(string)c["Name"]}.{(string)c["Type"]}.{(int)c["Size"]}.{(bool)c["AllowNull"]}")
				.ToArray();
			
			DataPackage ts = new DataPackage(headerSpaceSize, cis, null);
			JsonObject headers = (JsonObject)j["Headers"];
			foreach (var pair in headers)
			{
				ts.Headers.Add(pair.Key, pair.Value?.GetValue());
			}
			ts.UpdateHeaders();
			foreach (JsonObject r in rows)
			{
				ts.AddNew();
				int i = 0;
				foreach (var jo in r)
				{
					ts[i] = jo.Value?
						.GetValue()
						.CastOf(ts.GetFieldType(i));
					i++;
				}
				ts.Update();
			}
			ts.GoDataTop();
			return ts;
		}
	}
}