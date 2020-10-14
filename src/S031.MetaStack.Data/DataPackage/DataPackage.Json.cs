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
		/// <summary>
		/// Create <see cref="DataPackage"/> from full json format
		/// </summary>
		/// <param name="source">json string whit Headers, Columns and Data rows </param>
		/// <param name="jsonFormat"><see cref="TsJsonFormat"/></param>
		/// <param name="headerSpaceSize">For no headers package set headerSpaceSize = 8</param>
		internal DataPackage(JsonValue source, TsJsonFormat jsonFormat = TsJsonFormat.Full, int headerSpaceSize = 0) : base(source) 
		{
			if (jsonFormat != TsJsonFormat.Simple)
			{
				var j = source as JsonObject;
				if (headerSpaceSize == 0)
					headerSpaceSize = (int)j.GetIntOrDefault("HeaderSize", header_space_size_default);
				JsonArray columns = j.GetArray("Columns");
				JsonArray rows = j.GetArray("Rows");

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
					if (jsonFormat == TsJsonFormat.Full)
						foreach (JsonObject r in rows)
						{
							AddNew();
							foreach (var o in r)
								SetValue(o.Key, o.Value?.GetValue());
							Update();
						}
					else
					{
						//!!! Not tested
						foreach (JsonArray a in rows)
						{
							AddNew();
							foreach (var o in a)
								for (int i = 0; i < _colCount; i++)
									SetValue(_indexes[i], o?.GetValue());
							Update();
						}
					}
				}
			}
			else //simple
			{
				if (headerSpaceSize == 0)
					headerSpaceSize = header_space_size_default;

				_b = new BinaryDataBuffer(headerSpaceSize * 2);
				_bw = new BinaryDataWriter(_b);
				_br = new BinaryDataReader(_b);

				JsonObject dataRow;
				bool isArray = false;
				if (source is JsonObject j)
				{
					dataRow = j;
				}
				else if (source is JsonArray a)
				{
					if (a.Count == 0)
						throw new ArgumentException("Parameter array cannot be empty");
					dataRow = (JsonObject)a[0];
					isArray = true;
				}
				else
					throw new ArgumentException("The constructor of DataPackage requires JsonObject or JsonArray parameter type.");
				
				_headerSpaceSize = headerSpaceSize;
				_headers = new MapTable<string, object>(StringComparer.Ordinal);
				_colCount = dataRow.Count;
				_indexes = new string[_colCount];
				_colInfo = new ColumnInfo[_colCount];

				int i = 0;
				foreach (var kvp in dataRow)
				{
					_indexes[i] = kvp.Key;
					_colInfo[i] = ColumnInfo.FromValue(kvp.Value?.GetValue());
					i++;
				}
				WritePackageHeader();

				if (isArray)
				{
					foreach (var r in (source as JsonArray))
					{
						AddNew();
						foreach (var o in (JsonObject)r)
							SetValue(o.Key, o.Value?.GetValue());
						Update();
					}
				}
				else
				{
					AddNew();
					foreach (var o in dataRow)
						SetValue(o.Key, o.Value?.GetValue());
					Update();
				}
			}
			GoDataTop();
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

		/// <summary>
		/// Create  <see cref="DataPackage"/> from simple json format
		/// </summary>
		/// <param name="jsonString"></param>
		/// <param name="jsonFormat"><see cref="TsJsonFormat"/></param>
		/// <param name="headerSpaceSize">For no headers package set headerSpaceSize = 8</param>
		/// <returns></returns>
		public static DataPackage Parse(string jsonString, TsJsonFormat jsonFormat = TsJsonFormat.Simple, int headerSpaceSize = 8)
			=> new DataPackage(new JsonReader(jsonString).Read(), jsonFormat, headerSpaceSize);
		public static DataPackage Parse(string jsonString, TsJsonFormat jsonFormat = TsJsonFormat.Full)
			=> new DataPackage(new JsonReader(jsonString).Read(), jsonFormat);
		public override void FromJson(JsonValue source) { }
	}
}