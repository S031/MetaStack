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
	public sealed partial class DataPackage : JsonSerializible, IDataReader
	{
		public DataPackage(JsonValue source) : base(source) 
		{
			var j = source as JsonObject;
			int headerSpaceSize = (int)j["HeaderSize"];
			JsonArray columns = (j["Columns"] as JsonArray);
			JsonArray rows = (j["Rows"] as JsonArray);

			_b = new BinaryDataBuffer(headerSpaceSize * 2);
			_bw = new BinaryDataWriter(_b);
			_br = new BinaryDataReader(_b);

			_colCount = columns.Count;
			bool hasValues = rows != null;

			//headers
			_headerSpaceSize = headerSpaceSize;
			_headers = new MapTable<string, object>(StringComparer.Ordinal);
			JsonObject headers = (JsonObject)j["Headers"];
			foreach (var pair in headers)
				_headers.Add(pair.Key, pair.Value?.GetValue());

			//Create col info
			_indexes = new string[_colCount];
			_colInfo = new ColumnInfo[_colCount];
			for (int i = 0; i < _colCount; i++)
			{
				var c = columns[i];
				_indexes[i] = (string)c["Name"];
				_colInfo[i] = new ColumnInfo()
				{
					DataType = MdbTypeMap.GetType((string)c["Type"], typeof(string)), 
					ColumnSize = (int)c["Size"], 
					AllowDBNull = (bool)c["AllowNull"]
				};
			}
			WritePackageHeader();

			//values
			if (hasValues)
			{
				foreach (JsonObject r in rows)
				{
					AddNew();
					foreach (var o in r)
						SetValue(o.Key, o.Value?.GetValue());
					Update();
				}
				GoDataTop();
			}
		}
		/// <summary>
		/// serialize package to json format
		/// </summary>
		/// <param name="exportFormat">reqaired TsExportFormat.JSON</param>
		/// <returns>json formatted text</returns>
		public string ToString(TsExportFormat exportFormat)
		{
			if (exportFormat == TsExportFormat.JSON)
				return ToString();
			throw new NotImplementedException();
		}
		/// <summary>
		/// serialize current row to json format
		/// </summary>
		/// <returns>json formatted test</returns>
		public string GetStringRow()
		{
			JsonWriter writer = new JsonWriter(MetaStack.Json.Formatting.None);
			WriteRowJson(writer);
			return writer.ToString();
		}
		public string GetRowJSON()
			=> GetStringRow();
		protected override void ToJsonRaw(JsonWriter writer)
		{
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

		public static DataPackage Parse(int headerSpaceSize, string jsonString)
		{
			JsonValue jv = new JsonReader(jsonString).Read();
			DataPackage ts;
			if (jv is JsonObject j)
				ts = new DataPackage(headerSpaceSize,
					j.Select(kvp => kvp.Key).ToArray<string>(),
					j.Select(kvp => kvp.Value?.GetValue()).ToArray<object>());
			else // JsonArray
			{
				var a = (jv as JsonArray);
				if (a.Count == 0)
					throw new ArgumentException("Parameter array cannot be empty");
				j = (JsonObject)a[0];
				ts = new DataPackage(headerSpaceSize,
					j.Select(kvp => kvp.Key).ToArray<string>(), null);
				foreach (JsonObject r in a)
				{
					ts.AddNew();
					foreach (var o in r)
						ts.SetValue(o.Key, o.Value?.GetValue());
					ts.Update();
				}

			}
			ts.GoDataTop();
			return ts;
		}
		public static DataPackage Parse(string jsonString)
			=> new DataPackage(new JsonReader(jsonString).Read());
		public override void FromJson(JsonValue source) { }
	}
}