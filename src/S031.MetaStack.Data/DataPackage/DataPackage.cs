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
	public enum TsExportFormat
	{
		JSON,
		BINARY
	}

	public enum TsJsonFormat
	{
		Full,
		Array,
		Simple
	}

	public sealed partial class DataPackage : JsonSerializible, IDataReader
	{

		private struct ColumnInfo
		{
			public Type DataType;
			public int ColumnSize;
			public bool AllowDBNull;

			public static ColumnInfo FromValue(object value)
			{
				ColumnInfo c = new ColumnInfo();
				MdbTypeInfo info = MdbTypeMap.GetTypeInfo(value.GetType());
				c.DataType = info.Type;
				c.ColumnSize = info.Size;
				c.AllowDBNull = info.NullIfEmpty;
				return c;
			}

			// например ColumnInfo FromName("Field1.int.4.0")
			public static ColumnInfo FromName(string columnInfo)
			{
				string[] cInfo = (columnInfo + "....").Split('.');

				string typeName = cInfo[1].IsEmpty() ? "string" : cInfo[1].ToLower();
				Type t = MdbTypeMap.GetType(typeName, typeof(string));

				return new ColumnInfo()
				{
					DataType = t,
					ColumnSize = cInfo[2].IsEmpty() ? MdbTypeMap.GetTypeInfo(t).Size : cInfo[2].ToIntOrDefault(),
					AllowDBNull = cInfo[3].ToBoolOrDefault()
				};
			}
			public static ColumnInfo FromType(Type type)
			{
				return new ColumnInfo()
				{
					DataType = type,
					ColumnSize = MdbTypeMap.GetTypeInfo(type).Size,
					AllowDBNull = true
				};
			}

		}

		private const int header_pos = 10; // (sizeof(byte) + sizeof(int)) *2
		private const int header_space_size_default = 512;

		private int _headerSpaceSize;
		private readonly int _colCount;
		private int _dataPos;
		private int _rowPos;

		private object[] _dataRow;
		private readonly MapTable<string, object> _headers;
		private readonly string[] _indexes;
		private readonly ColumnInfo[] _colInfo;

		private readonly BinaryDataBuffer _b;
		private readonly BinaryDataWriter _bw;
		private readonly BinaryDataReader _br;

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from <see cref="byte"/> array
		/// using when deserialise <see cref="DataPackage"/> from array created with <see cref="DataPackage.ToArray()"/> methos
		/// </summary>
		/// <param name="data"><see cref="byte"/> array created with <see cref="DataPackage.ToArray()"/> methos</param>
		public DataPackage(byte[] source):base(null)
		{
			_b = (BinaryDataBuffer)source;
			_br = new BinaryDataReader(_b);
			_bw = new BinaryDataWriter(_b);

			//Read sizes
			_headerSpaceSize = _br.Read<int>();
			if (_headerSpaceSize < 0)
				throw new DataPackageMessageFormatException("Invalid header size");

			_colCount = _br.Read<int>();
			if (_colCount <= 0 || _colCount > 64192)
				throw new DataPackageMessageFormatException("Invalid columns count, must be more thrn 0 and less then 64192");

			//Read headers
			_headers = new MapTable<string, object>(StringComparer.Ordinal);
			_br.ReadNext(); //Must be Object
			_br.ReadRaw(_headers);
			_br.Position = _headerSpaceSize + header_pos;

			//Read columns
			_indexes = new string[_colCount];
			_colInfo = new ColumnInfo[_colCount];
			for (int i = 0; i < _colCount; i++)
			{
				_br.ReadNext();
				string name = _br.ReadString();
				if (string.IsNullOrEmpty(name))
					throw new DataPackageMessageFormatException($"Invalid column name for column index {i}");

				_indexes[i] = name;
				_colInfo[i] = ReadColumnInfo(_br);
			}
			_dataPos = _br.Position;
		}

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from <see cref="IDataReader"/>
		/// </summary>
		/// <param name="dr"><see cref="IDataReader"/></param>
		/// <returns><see cref="byte[]"/></returns>
		public DataPackage(IDataReader dr) : this(dr, header_space_size_default, false) { }

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from <see cref="IDataReader"/>
		/// </summary>
		/// <param name="dr"><see cref="IDataReader"/></param>
		/// <param name="headerSpaceSize">Header size (default 512 bytes)</param>
		/// <param name="allowDublicate"><see cref="bool"/></param>
		/// <returns></returns>
		public DataPackage(IDataReader dr, int headerSpaceSize, bool allowDublicate):base(null)
		{
			_headerSpaceSize = headerSpaceSize <= 0 ? header_space_size_default : headerSpaceSize;
			_b = new BinaryDataBuffer(_headerSpaceSize * 2);
			_bw = new BinaryDataWriter(_b);
			_br = new BinaryDataReader(_b);
			_headers = new MapTable<string, object>(StringComparer.Ordinal);

			using (DataTable dt = dr.GetSchemaTable())
			{
				_colCount = dr.FieldCount;
				_indexes = new string[_colCount];
				_colInfo = new ColumnInfo[_colCount];
				
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					DataRow drCol = dt.Rows[i];
					string name = (string)drCol["ColumnName"];

					if (allowDublicate)
					{
						string nameOrig = name;
						for (int j = 1; _indexes.Contains(name, StringComparer.OrdinalIgnoreCase); j++)
							name = nameOrig + j.ToString();
					}
					else if (_indexes.Contains(name, StringComparer.OrdinalIgnoreCase))
						continue;

					ColumnInfo ci = new ColumnInfo()
					{
						DataType = (Type)drCol["DataType"],
						ColumnSize = (int)drCol["ColumnSize"],
						AllowDBNull = (bool)drCol["AllowDBNull"]
					};
					_indexes[i] = name;
					_colInfo[i] = ci;
				}

				WritePackageHeader();
				
				//write data
				for (; dr.Read();)
				{
					object[] values = new object[_colCount];
					dr.GetValues(values);
					//_bw.Write(values);
					AddNew();
					SetValues(values);
					Update();
				}
				GoDataTop();
			}
		}

		private void WritePackageHeader(byte[] headersAsByteArray = null)
		{
			//Write sizes
			_bw.Position = 0;
			_bw.Write(_headerSpaceSize);
			_bw.Write(_colCount);

			//Write Headers
			if (headersAsByteArray == null)
				_bw.Write(_headers);
			else
				_bw.WriteBlock(headersAsByteArray);

			int len = header_pos + _headerSpaceSize - _bw.Position;

			if (len < 0)
			{
				int p = _bw.Position;
				//if _headerSpaceSize less then sizeof _headers
				_headerSpaceSize = p;
				len = header_pos + _headerSpaceSize - p;
				//write new _headerSpaceSize
				_bw.Position = 0;
				_bw.Write(_headerSpaceSize);
				_bw.Position = p;
			}
			_bw.WriteSpace(len);

			//Write ColInfo
			for (int i = 0; i < _colCount; i++)
			{
				var ci = _colInfo[i];
				_bw.Write(_indexes[i]);
				_bw.Write((byte)MdbTypeMap.GetTypeInfo(ci.DataType).MdbType);
				_bw.Write(ci.ColumnSize);
				_bw.Write(ci.AllowDBNull);
			}
			_dataPos = _bw.Position;
		}

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from list of columns definitions
		/// </summary>
		/// <param name="columns">list of columns definitions strings  in format ColimnName[.ColumnType][.ColumnWidth]</param>
		/// <example>
		/// <code>
		/// p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
		/// </code>
		/// </example>
		public DataPackage(params string[] columns) :
			this(
				headerSpaceSize: header_space_size_default,
				columns: columns,
				values: null)
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from list of objects in key/value pair sequence
		/// </summary>
		/// <param name="keyValuePair">list of objects in key/value pair sequence</param>
		/// <example>
		/// <code>
		/// p = new DataPackage("Col1", 999999999999, "Col2", "Свойство 1", "Col3", DateTime.Now, "Col4", Guid.NewGuid());
		/// </code>
		/// </example>
		public DataPackage(params object[] keyValuePair) :
			this(
				headerSpaceSize: header_space_size_default,
				columns: keyValuePair.Where((obj, i) => i % 2 == 0).Select(obj => (string)obj).ToArray(),
				values: keyValuePair.Where((obj, i) => i % 2 != 0).ToArray())
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from list of columns definitions and list of values
		/// </summary>
		/// <param name="columns">list of columns definitions strings  in format ColimnName[.ColumnType][.ColumnWidth]</param>
		/// <param name="values">list of values</param>
		/// <example>
		/// <code>
		/// DataPackage p = new DataPackage(new string[] { "Col1.long", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34" },
		///		new object[] { 999999999999, "Property one", DateTime.Now, Guid.NewGuid()});
		///	</code>
		/// </example>
		public DataPackage(string[] columns, object[] values) :
			this(
				headerSpaceSize: header_space_size_default,
				columns: columns, 
				values: values)
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> with headerSpaceSize from list of columns definitions and list of values
		/// </summary>
		/// <param name="headerSpaceSize">Header size (default 512 bytes)</param>
		/// <param name="columns">list of columns definitions strings  in format ColimnName[.ColumnType][.ColumnWidth]</param>
		/// <param name="values">list of values</param>
		public DataPackage(int headerSpaceSize, string[] columns, object[] values):base(null)
		{
			_headerSpaceSize = headerSpaceSize <= 0 ? header_space_size_default : headerSpaceSize;
			_b = new BinaryDataBuffer(_headerSpaceSize * 2);
			_bw = new BinaryDataWriter(_b);
			_br = new BinaryDataReader(_b);

			_colCount = columns.Length;
			bool hasValues = values != null && values.Length == _colCount;
			
			_headers = new MapTable<string, object>(StringComparer.Ordinal);

			//Create col info
			_indexes = new string[_colCount];
			_colInfo = new ColumnInfo[_colCount];
			for (int i = 0; i < _colCount; i++)
			{
				ColumnInfo info;
				string name;
				if (columns[i].IndexOf('.') > 0)
				{
					info = ColumnInfo.FromName(columns[i]);
					name = columns[i].GetToken(0, ".");
				}
				else if (hasValues)
				{
					name = columns[i];
					info = ColumnInfo.FromValue(values[i]);
				}
				else
				{
					name = columns[i];
					info = ColumnInfo.FromType(typeof(string));
				}
				_indexes[i] = name;
				_colInfo[i] = info;
			}
			
			WritePackageHeader();

			if (hasValues)
			{
				AddNew();
				SetValues(values);
				Update();
			}
			else if (values != null)
				throw new ArgumentException("The length of the data array must be equal to the length of the array of field names");
		}

		public DataPackage SetValue(string name, object value)
		{
			_dataRow[GetOrdinal(name)] = value;
			return this;
		}

		public DataPackage SetValue(int index, object value)
		{
			_dataRow[index] = value;
			return this;
		}

		public DataPackage SetValues(params object[] values)
		{
			if (values.Length != _colCount)
				throw new InvalidOperationException($"Size array of parameters must be equals columns size");
			_dataRow = values;
			return this;
		}

		public int GetOrdinal(string name) => _indexes.IndexOf(name, StringComparer.Ordinal);

		/// <summary>
		/// Add a new row to <see cref="DataPackage"/>
		/// </summary>
		public DataPackage AddNew()
		{
			_dataRow = new object[_colCount];
			_rowPos = _bw.Length;
			return this;
		}

		/// <summary>
		/// Add new row or Updates the current row with a rewrite to end of array
		/// </summary>
		public DataPackage Update()
		{
			if (_rowPos != 0 && _rowPos >= _bw.Length)
				//Добавление новой строки
				WriteRow();
			else if (_rowPos != 0)
			{
				throw new InvalidOperationException("Update method applies only for added data");
				//Обновление существующей с перезаписью до конца потока
				//
				//int count = (int)(_ms.Length - _ms.Position);
				//byte[] nextData = new byte[count];
				//_ = _ms.Read(nextData, 0, count);
				//WriteRow();
				//long l = _ms.Position;
				//_ms.Write(nextData, 0, (int)nextData.Length);
				//if (_ms.Position < _ms.Length)
				//	_ms.SetLength(_ms.Position);
				//_ms.Seek(l, SeekOrigin.Begin);
			}
			return this;
		}

		void WriteRow()
		{
			_bw.Position = _rowPos;
			for (int i = 0; i < _colCount; i++)
				_bw.Write(_dataRow[i]);

		}

		/// <summary>
		/// Serialize <see cref="DataPackage"/> to <see cref="byte[]"/> array
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray() => _bw.GetBytes();

		/// <summary>
		/// Seek and select first data row
		/// </summary>
		public DataPackage GoDataTop()
		{
			_bw.Position = _dataPos;
			return this;
		}

		/// <summary>
		/// Close and dispose the <see cref="DataPackage"/>
		/// </summary>
		public void Close()
			=> _b.Dispose();

		/// <summary>
		/// Get <see cref="DataPackage "/> headers as Dictionary<string, object>
		/// </summary>
		public IDictionary<string, object> Headers => _headers;

		public DataPackage SetHeader(string key, object value)
		{
			_headers[key] = value;
			return this;
		}

		/// <summary>
		/// Commit changes maded in headers data
		/// </summary>
		/// <returns></returns>
		public DataPackage UpdateHeaders()
		{
			_bw.Position = header_pos;
			int limit = header_pos + _headerSpaceSize;

			BinaryDataBuffer b = new BinaryDataBuffer(_headerSpaceSize);
			BinaryDataWriter bw = new BinaryDataWriter(b);
			bw.Write(_headers);
			if (bw.Length <= _headerSpaceSize)
				_bw.WriteBlock(b);
			else if (_dataPos >= _b.Length)
				// No data rewrite package header only
				WritePackageHeader(b);
			else
			{
				// rewrite package header and shift data 
				byte[] data = _b.Slice(_dataPos, _b.Length - _dataPos).GetBytes();
				WritePackageHeader(b);
				_bw.WriteBlock(data);
			}
			return this;
		}

		ColumnInfo ReadColumnInfo(BinaryDataReader br)
		{
			try
			{
				ColumnInfo ci = new ColumnInfo()
				{
					DataType = MdbTypeMap.GetType((MdbType)br.Read<byte>()),
					ColumnSize = br.Read<int>(),
					AllowDBNull = br.Read<bool>()
				};
				return ci;
			}
			catch (Exception e)
			{
				throw new DataPackageMessageFormatException(e.Message);
			}
		}

		/// <summary>
		/// <see cref="IDataReader.Depth"/>not supported
		/// </summary>
		public int Depth
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// <see cref="IDataReader.GetSchemaTable"/> not supported in NET CORE version
		/// </summary>
		/// <returns></returns>
		public DataTable GetSchemaTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn() { ColumnName = "ColumnName", DataType = typeof(string) });
			dt.Columns.Add(new DataColumn() { ColumnName = "ColumnOrdinal", DataType = typeof(int) });
			dt.Columns.Add(new DataColumn() { ColumnName = "ColumnSize", DataType = typeof(int) });
			dt.Columns.Add(new DataColumn() { ColumnName = "NumericPrecision", DataType = typeof(int) });
			dt.Columns.Add(new DataColumn() { ColumnName = "NumericScale", DataType = typeof(int) });
			dt.Columns.Add(new DataColumn() { ColumnName = "DataType", DataType = typeof(Type) });
			dt.Columns.Add(new DataColumn() { ColumnName = "ProviderType", DataType = typeof(Type) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsLong", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "AllowDBNull", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsReadOnly", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsRowVersion", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsUnique", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsKey", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "IsAutoIncrement", DataType = typeof(bool) });
			dt.Columns.Add(new DataColumn() { ColumnName = "BaseSchemaName", DataType = typeof(string) });
			dt.Columns.Add(new DataColumn() { ColumnName = "BaseCatalogName", DataType = typeof(string) });
			dt.Columns.Add(new DataColumn() { ColumnName = "BaseTableName", DataType = typeof(string) });
			dt.Columns.Add(new DataColumn() { ColumnName = "BaseColumnName", DataType = typeof(string) });

			for (int i = 0; i < _colCount; i++)
			{
				ColumnInfo c = _colInfo[i];
				DataRow dr = dt.NewRow();
				dr["ColumnName"] = _indexes[i];
				dr["ColumnOrdinal"] = i;
				dr["ColumnSize"] = c.ColumnSize;
				dr["NumericPrecision"] = c.DataType.IsNumeric(NumericTypesScope.FloatingPoint) ? (int)c.ColumnSize : 0;
				dr["NumericScale"] = c.DataType.IsNumeric(NumericTypesScope.FloatingPoint) ? (int)c.ColumnSize : 0;
				dr["DataType"] = c.DataType;
				dr["ProviderType"] = null;
				dr["IsLong"] = c.DataType.IsNumeric(NumericTypesScope.Integral);
				dr["AllowDBNull"] = c.AllowDBNull;
				dr["IsReadOnly"] = false;
				dr["IsRowVersion"] = false;
				dr["IsUnique"] = false;
				dr["IsKey"] = false;
				dr["IsAutoIncrement"] = false;
				dr["BaseSchemaName"] = null;
				dr["BaseTableName"] = null;
				dr["BaseColumnName"] = _indexes[i];
				dt.Rows.Add(dr);
			}
			return dt;
		}

		/// <summary>
		/// Get Closed state of <see cref="DataPackage"/>
		/// </summary>
		public bool IsClosed => (_b == null);

		/// <summary>
		/// <see cref="DataPackage.GoDataTop()"/>
		/// </summary>
		/// <returns></returns>
		public bool NextResult()
		{
			this.GoDataTop();
			return this.Read();
		}

		/// <summary>
		/// <see cref="IDataReader.Read()"/> advance <see cref="DataPackage"/> to the next record
		/// </summary>
		/// <returns></returns>
		public bool Read()
		{
			if (_br.EOF)
			{
				_dataRow = null;
				return false;
			}
			_dataRow = _br.ReadValues(_colCount);
			return true;
		}

		/// <summary>
		/// <see cref="IDataReader.RecordsAffected()"/> Not supported
		/// </summary>
		public int RecordsAffected
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing)
				Close();
		}

		/// <summary>
		/// Get count of columns (fields)
		/// </summary>
		public int FieldCount => _colCount;

		public bool GetBoolean(int i) => (bool)_dataRow[i];

		public byte GetByte(int i) => (byte)_dataRow[i];

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			byte[] data = (byte[])_dataRow[i];
			buffer = new byte[length];
			Array.Copy(data, fieldOffset, buffer, bufferoffset, length);
			return buffer.Length;
		}

		public char GetChar(int i) => (char)_dataRow[i];

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i) => MdbTypeMap.GetTypeInfo(_colInfo[i].DataType).Name;

		public DateTime GetDateTime(int i) => (DateTime)_dataRow[i];

		public decimal GetDecimal(int i) => (decimal)_dataRow[i];

		public double GetDouble(int i) => (double)_dataRow[i];

		public Type GetFieldType(int i) => _colInfo[i].DataType;

		public float GetFloat(int i) => (float)_dataRow[i];

		public Guid GetGuid(int i) => (Guid)_dataRow[i];

		public short GetInt16(int i) => (Int16)_dataRow[i];

		public int GetInt32(int i) => (Int32)_dataRow[i];

		public long GetInt64(int i) => (Int64)_dataRow[i];

		public string GetName(int i) => _indexes[i];

		public string GetString(int i) => (string)_dataRow[i];

		public object GetValue(int i) => _dataRow[i];

		public int GetValues(object[] values)
		{
			int i = 0;
			foreach (object value in _dataRow)
			{
				values[i] = value;
				i++;
			}
			return i;
		}

		public bool IsDBNull(int i) => DBNull.Value.Equals(_dataRow[i]);

		public object this[string name]
		{
			get { return _dataRow[GetOrdinal(name)]; }
			set { _dataRow[GetOrdinal(name)] = value; }
			//get { return _dataRow[_indexes.IndexOf(name, StringComparer.Ordinal)]; }
			//set { _dataRow[_indexes.IndexOf(name, StringComparer.Ordinal)] = value; }
		}

		public object this[int i]
		{
			get { return _dataRow[i]; }
			set { _dataRow[i] = value; }
		}

		public DataTable ToDataTable()
		{
			var dt = new DataTable();
			for (int i = 0; i < _indexes.Length; i++)
			{
				ColumnInfo ci = _colInfo[i];
				DataColumn dc = new DataColumn(_indexes[i], ci.DataType);
				if (ci.DataType == typeof(string))
					dc.MaxLength = ci.ColumnSize;
				dc.AllowDBNull = ci.AllowDBNull;
				dt.Columns.Add(dc);
			}
			GoDataTop();
			for (; Read();)
				dt.Rows.Add(_dataRow);
			GoDataTop();
			return dt;
		}

		public class DataPackageMessageFormatException : Exception
		{
			public DataPackageMessageFormatException() : base() { }
			public DataPackageMessageFormatException(string message) : base(message) { }
		}

	}
}
