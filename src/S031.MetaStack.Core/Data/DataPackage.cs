using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using S031.MetaStack.Common;
using S031.MetaStack.Json;
#if NETCOREAPP
using S031.MetaStack.Core.Json;
#else
using S031.MetaStack.WinForms.Json;
#endif
using System.Threading.Tasks;
#if SERIALIZEBINARY
using MessagePack;
#endif

#if NETCOREAPP
namespace S031.MetaStack.Core.Data
#else
namespace S031.MetaStack.WinForms.Data
#endif
{
	public enum TsExportFormat
	{
		JSON,
		XML
	}

	//public enum BinaryMode
	//{
	//	simple,
	//	full
	//}

	public partial class DataPackage : IDataReader
	{
		private struct DataTypeInfo
		{
			public MdbType ID;
			public Action<BinaryWriter, object> WriteDelegate;
			public Func<BinaryReader, object> ReadDelegate;
		}
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
				//int i = _listTypeNames.IndexOf(typeName);
				//Type t = i > -1 ? _listTypes[i] : typeof(string);
				Type t = MdbTypeMap.GetType(typeName, typeof(string));

				return new ColumnInfo()
				{
					DataType = t,
					//ColumnSize = cInfo[2].IsEmpty() ? _listTypeSizes[_listTypes.IndexOf(t)] : cInfo[2].ToIntOrDefault(),
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

		static readonly Dictionary<Type, DataTypeInfo> _dti = new Dictionary<Type, DataTypeInfo>()
		{
			{typeof(bool), new DataTypeInfo(){ID = MdbType.@bool, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@bool); bw.Write((bool)obj); }, ReadDelegate = br => br.ReadBoolean()}},
			{typeof(char), new DataTypeInfo(){ID = MdbType.@char, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@char); bw.Write((char)obj); }, ReadDelegate = br => br.ReadChar()}},
			{typeof(byte), new DataTypeInfo(){ID = MdbType.@byte, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@byte);bw.Write((byte)obj); }, ReadDelegate = br => br.ReadByte()}},
			{typeof(sbyte), new DataTypeInfo(){ID = MdbType.@sbyte, WriteDelegate = (bw, obj) => {   bw.Write((byte)MdbType.@sbyte); bw.Write((sbyte)obj); }, ReadDelegate = br => br.ReadSByte()}},
			{typeof(short), new DataTypeInfo(){ID = MdbType.@short, WriteDelegate = (bw, obj) => {   bw.Write((byte)MdbType.@short); bw.Write((Int16)obj); }, ReadDelegate = br => br.ReadInt16()}},
			{typeof(ushort), new DataTypeInfo(){ID = MdbType.@ushort, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@ushort); bw.Write((UInt16)obj); }, ReadDelegate = br => br.ReadUInt16()}},
			{typeof(int), new DataTypeInfo(){ID = MdbType.@int, WriteDelegate = (bw, obj) => {   bw.Write((byte)MdbType.@int); bw.Write((Int32)obj); }, ReadDelegate = br => br.ReadInt32()}},
			{typeof(uint), new DataTypeInfo(){ID = MdbType.@uint, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@uint); bw.Write((UInt32)obj); }, ReadDelegate = br => br.ReadUInt16()}},
			{typeof(long), new DataTypeInfo(){ID = MdbType.@long, WriteDelegate = (bw, obj) => {   bw.Write((byte)MdbType.@long); bw.Write((Int64)obj); }, ReadDelegate = br => br.ReadInt64()}},
			{typeof(ulong), new DataTypeInfo(){ID = MdbType.@ulong, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@ulong); bw.Write((UInt64)obj); }, ReadDelegate = br => br.ReadUInt16()}},
			{typeof(float), new DataTypeInfo(){ID = MdbType.@float, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@float); bw.Write((Single)obj); }, ReadDelegate = br => br.ReadSingle()}},
			{typeof(double), new DataTypeInfo(){ID = MdbType.@double, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@double); bw.Write((double)obj); }, ReadDelegate = br => br.ReadDouble()}},
			{typeof(decimal), new DataTypeInfo(){ID = MdbType.@decimal, WriteDelegate = (bw, obj) => {   bw.Write((byte)MdbType.@decimal); bw.Write((decimal)obj); }, ReadDelegate = br => br.ReadDecimal()}},
			{typeof(DateTime), new DataTypeInfo(){ID = MdbType.dateTime, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.dateTime); bw.Write(((DateTime)obj).ToBinary()); }, ReadDelegate = br => DateTime.FromBinary(br.ReadInt64())}},
			{typeof(string), new DataTypeInfo(){ID = MdbType.@string, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.@string); bw.Write((string)obj); }, ReadDelegate = br => br.ReadString()}},
			{typeof(byte[]), new DataTypeInfo(){ID = MdbType.byteArray, WriteDelegate = (bw, obj) => {  bw.Write((byte)MdbType.byteArray); WriteByteArray(bw, (byte[])obj); }, ReadDelegate = ReadByteArray}},
			{typeof(char[]), new DataTypeInfo(){ID = MdbType.charArray, WriteDelegate = (bw, obj) => {  bw.Write((byte)MdbType.charArray);WriteCharArray(bw, (char[])obj); }, ReadDelegate = ReadCharArray}},
			{typeof(Guid), new DataTypeInfo(){ID = MdbType.guid, WriteDelegate = (bw, obj) => { bw.Write((byte)MdbType.guid); WriteByteArray(bw, ((Guid)obj).ToByteArray()); }, ReadDelegate = br => new Guid(ReadByteArray(br))}},
#if SERIALIZEBINARY
			//For fast optimization serialization
			//Required https://www.nuget.org/packages/MessagePack/
			{ typeof(object), new DataTypeInfo(){ID = MdbType.@object, WriteDelegate = (bw, obj) => {
				bw.Write((byte)MdbType.@object); WriteByteArray(bw, MessagePackSerializer.Typeless.Serialize(obj)); },
				ReadDelegate = br => MessagePackSerializer.Typeless.Deserialize(ReadByteArray(br))}}
#else
			{typeof(object), new DataTypeInfo(){ID = MdbType.@object, WriteDelegate = (bw, obj) => {
				bw.Write((byte)MdbType.@object); bw.Write(JSONExtensions.SerializeObject(obj)); },
				ReadDelegate = br => JSONExtensions.DeserializeObject(br.ReadString())}}
#endif
		};
		private static readonly object obj4Lock = new object();
		private MemoryStream _ms;
		private BinaryReader _br;
		private readonly BinaryWriter _bw;

		//string _tableName;
		private readonly int _headerSpaceSize;
		private readonly int _colCount;
		private const int _headerPos = 8; // sizeof(int) *2
		private const int _headerSpaceSizeDef = 512;
		private readonly long _dataPos;
		private long _rowPos;

		private Dictionary<string, object> _dataRow;
		private readonly Dictionary<string, object> _headers;
		private readonly List<string> _indexes;
		private readonly List<ColumnInfo> _colInfo;
		
		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> from <see cref="byte"/> array
		/// using when deserialise <see cref="DataPackage"/> from array created with <see cref="DataPackage.ToArray()"/> methos
		/// </summary>
		/// <param name="data"><see cref="byte"/> array created with <see cref="DataPackage.ToArray()"/> methos</param>
		public DataPackage(byte[] data) :
			this(new MemoryStream(data))
		{
		}

		DataPackage(MemoryStream ms)
		{
			_ms = ms;
			_ms.Seek(0, SeekOrigin.Begin);
			_br = new BinaryReader(_ms);
			_bw = new BinaryWriter(_ms);
			_headerSpaceSize = _br.ReadInt32();
			if (_headerSpaceSize < 0)
				throw new DataPackageMessageFormatException("Invalid header size");
			_colCount = _br.ReadInt32();
			if (_colCount <=0)
				throw new DataPackageMessageFormatException("Invalid column count");
			//Headers start
			int headCount = _br.ReadInt32();
			_headers = new Dictionary<string, object>();
			for (int i = 0; i < headCount; i++)
			{
				string key = _br.ReadString();
				MdbType t = (MdbType)_br.ReadByte();
				if (t == MdbType.@null)
					_headers.Add(key, null);
				else
					_headers.Add(key, _dti[t.Type()].ReadDelegate(_br));
			}
			_ms.Seek(_headerSpaceSize + _headerPos -1, SeekOrigin.Begin);

			// ColInfo start
			_indexes = new List<string>(_colCount);
			_colInfo = new List<ColumnInfo>(_colCount);
			for (int i = 0; i < _colCount; i++)
			{
				string name = _br.ReadString();
				if (name.IsEmpty())
					throw new DataPackageMessageFormatException($"Invalid column name for column index {i}");
				_indexes.Add(name);
				_colInfo.Add(ReadColumnInfo(_br));
			}
			_dataPos = _ms.Position;
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
			this(WriteDataInternal(_headerSpaceSizeDef, columns, null))
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
		public DataPackage(params object[] keyValuePair)
			: this(WriteDataInternal(_headerSpaceSizeDef, keyValuePair.Where((obj, i) => i % 2 == 0).Select(obj => (string)obj).ToArray(),
			keyValuePair.Where((obj, i) => i % 2 != 0).ToArray()))
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
			this(WriteDataInternal(_headerSpaceSizeDef, columns, values))
		{
		}
		/// <summary>
		/// Create a new instance of <see cref="DataPackage"/> with headerSpaceSize from list of columns definitions and list of values
		/// </summary>
		/// <param name="headerSpaceSize">Header size (default 512 bytes)</param>
		/// <param name="columns">list of columns definitions strings  in format ColimnName[.ColumnType][.ColumnWidth]</param>
		/// <param name="values">list of values</param>
		public DataPackage(int headerSpaceSize, string[] columns, object[] values) :
			this(WriteDataInternal(headerSpaceSize, columns, values))
		{
		}
		/// <summary>
		/// Create a serialised in <see cref="byte[]"/> array <see cref="DataPackage"/> see <see cref="DataPackage(string[], object[])"/>
		/// </summary>
		/// <param name="headerSpaceSize">Header size (default 512 bytes)</param>
		/// <param name="columns">list of columns definitions strings  in format ColimnName[.ColumnType][.ColumnWidth]</param>
		/// <param name="values">list of values</param>
		/// <returns><see cref="byte[]"/></returns>
		public static byte[] WriteData(int headerSpaceSize, string[] columns, object[] values)=>
			WriteDataInternal(headerSpaceSize, columns, values).ToArray();

		static MemoryStream WriteDataInternal(int headerSpaceSize, string[] columns, object[] values)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				int fieldCount = columns.Length;
				bool hasValues = values != null && values.Length == fieldCount;

				bw.Write(headerSpaceSize);
				bw.Write(fieldCount);
				//Write Headers
				bw.Write(0);
				bw.Write(new byte[headerSpaceSize - 5]);
				//Write ColInfo
				for (int i = 0; i < fieldCount; i++)
				{
					if (columns[i].IndexOf('.') > 0)
					{
						bw.Write(columns[i].GetToken(0, "."));
						WriteColumnInfo(bw, ColumnInfo.FromName(columns[i]));
					}
					else if (hasValues)
					{
						bw.Write(columns[i]);
						WriteColumnInfo(bw, ColumnInfo.FromValue(values[i]));
					}
					else
					{
						bw.Write(columns[i]);
						WriteColumnInfo(bw, ColumnInfo.FromType(typeof(string)));
					}

				}
				if (hasValues)
				{
					for (int i = 0; i < fieldCount; i++)
					{
						object value = values[i];
						if (value == null || DBNull.Value.Equals(value))
							bw.Write((byte)MdbType.@null);
						else
							lock (obj4Lock)
								_dti[MdbTypeMap.GetType(value.GetType())].WriteDelegate(bw, value);
					}
				}
				else if (values != null)
					//The length of the data array must be equal to the length of the array of field names
					throw new ArgumentException(Properties.Strings.S031_MetaStack_Core_Data_writeData_1);
				return ms;
			}
		}
		/// <summary>
		/// Create a serialised in <see cref="byte[]"/> array <see cref="DataPackage"/> from <see cref="IDataReader"/>"/>
		/// </summary>
		/// <param name="dr"><see cref="IDataReader"/></param>
		/// <returns><see cref="byte[]"/></returns>
		public static byte[] WriteData(IDataReader dr) => WriteData(dr, _headerSpaceSizeDef, false);
		/// <summary>
		/// Create a serialised in <see cref="byte[]"/> array <see cref="DataPackage"/> from <see cref="IDataReader"/>"/>
		/// </summary>
		/// <param name="dr"><see cref="IDataReader"/></param>
		/// <param name="headerSpaceSize">Header size (default 512 bytes)</param>
		/// <param name="allowDublicate"><see cref="bool"/></param>
		/// <returns></returns>
		public static byte[] WriteData(IDataReader dr, int headerSpaceSize, bool allowDublicate)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{

				//int fieldCount = dr.FieldCount;
				//Dictionary<string, ColumnInfo> colInfo = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);

				//for (int i = 0; i < fieldCount; i++)
				//{
				//	string name = dr.GetName(i);

				//	if (allowDublicate)
				//	{
				//		string nameOrig = name;
				//		for (int j = 1; colInfo.ContainsKey(name); j++)
				//			name = nameOrig + j.ToString();
				//	}
				//	else if (colInfo.ContainsKey(name))
				//		continue;

				//	colInfo.Add(name, ColumnInfo.FromType(dr.GetFieldType(i)));
				//}

				DataTable dt = dr.GetSchemaTable();
				int fieldCount = dr.FieldCount;
				Dictionary<string, ColumnInfo> colInfo = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);

				for (int i = 0; i < dt.Rows.Count; i++)
				{
					DataRow drCol = dt.Rows[i];
					string name = (string)drCol["ColumnName"];

					if (allowDublicate)
					{
						string nameOrig = name;
						for (int j = 1; colInfo.ContainsKey(name); j++)
							name = nameOrig + j.ToString();
					}
					else if (colInfo.ContainsKey(name))
						continue;

					ColumnInfo ci = new ColumnInfo()
					{
						DataType = (Type)drCol["DataType"],
						ColumnSize = (int)drCol["ColumnSize"],
						AllowDBNull = (bool)drCol["AllowDBNull"]
					};
					colInfo.Add(name, ci);
				}

				bw.Write(headerSpaceSize);
				bw.Write(colInfo.Count);
				//Write Headers
				bw.Write(0); //4 bytes
				bw.Write(new byte[headerSpaceSize - 5]);
				//Write colInfo
				foreach (KeyValuePair<string, ColumnInfo> kvp in colInfo)
				{
					bw.Write(kvp.Key);
					WriteColumnInfo(bw, kvp.Value);
				}


				for (; dr.Read();)
				{
					foreach (KeyValuePair<string, ColumnInfo> kvp in colInfo)
					{
						object value = dr[kvp.Key];
						if (value == null || DBNull.Value.Equals(value))
							bw.Write((byte)MdbType.@null);
						else
							_dti[MdbTypeMap.GetType(value.GetType())].WriteDelegate(bw, value);
					}
				}
				return ms.ToArray();
			}
		}
		/// <summary>
		/// Add new row or Updates the current row with a rewrite to end of array
		/// </summary>
		public DataPackage Update()
		{
			if (_rowPos != 0 && _rowPos >= _ms.Length)
				//Добавление новой строки
				WriteRow();
			else if (_rowPos != 0)
			{
				//Обновление существующей с перезаписью до конца потока
				//
				int count = (int)(_ms.Length - _ms.Position);
				byte[] nextData = new byte[count];
				_ = _ms.Read(nextData, 0, count);
				WriteRow();
				long l = _ms.Position;
				_ms.Write(nextData, 0, (int)nextData.Length);
				if (_ms.Position < _ms.Length)
					_ms.SetLength(_ms.Position);
				_ms.Seek(l, SeekOrigin.Begin);
			}
			return this;
		}
		/// <summary>
		/// Using only if update existing row
		/// </summary>
		/// <returns></returns>
		public async Task UpdateAsync()
		{
			if (_rowPos != 0 && _rowPos >= _ms.Length)
				//Add new row
				WriteRow();
			else if (_rowPos != 0)
			{
				//Updates the current row with a rewrite to end of array
				int count = (int)(_ms.Length - _ms.Position);
				byte[] nextData = new byte[count];
				_ = await _ms.ReadAsync(nextData, 0, count).ConfigureAwait(false);
				WriteRow();
				long l = _ms.Position;
				await _ms.WriteAsync(nextData, 0, (int)nextData.Length).ConfigureAwait(false);
				if (_ms.Position < _ms.Length)
					_ms.SetLength(_ms.Position);
				_ms.Seek(l, SeekOrigin.Begin);
			}
		}
		void WriteRow()
		{
			_ms.Seek(_rowPos, SeekOrigin.Begin);
			for (int i = 0; i < _colCount; i++)
			{
				object value = _dataRow[_indexes[i]];
				if (value == null || DBNull.Value.Equals(value))
					_bw.Write((byte)MdbType.@null);
				else
					_dti[MdbTypeMap.GetType(value.GetType())].WriteDelegate(_bw, value);
			}
		}
		/// <summary>
		/// Serialize <see cref="DataPackage"/> to <see cref="byte[]"/> array
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray() => _ms.ToArray();
		/// <summary>
		/// Seek and select first data row
		/// </summary>
		public DataPackage GoDataTop()
		{
			_ms.Seek(_dataPos, SeekOrigin.Begin);
			return this;
		}
		/// <summary>
		/// Close and dispose the <see cref="DataPackage"/>
		/// </summary>
		public void Close() => this.Dispose();
		/// <summary>
		/// Get <see cref="DataPackage "/> headers as Dictionary<string, object>
		/// </summary>
		public Dictionary<string, object> Headers => _headers; 
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
			_ms.Seek(_headerPos, SeekOrigin.Begin);
			_bw.Write(_headers.Count);
			int limit = _headerPos + _headerSpaceSize;
			foreach (var kvp in _headers)
			{
				object value = kvp.Value;
				bool isNull = value == null || DBNull.Value.Equals(value);
				MdbTypeInfo t = MdbTypeMap.GetTypeInfo(value.GetType());
				//for unicode
				int len = kvp.Key.Length * 2 + 1;
				if (!isNull)
				{
					if (t.Size < 255)
						len += t.Size;
					else if (t.MdbType == MdbType.@string)
						len += ((string)value).Length * 2;
					else if (t.MdbType == MdbType.byteArray)
						len += ((byte[])value).Length;
					else if (t.MdbType == MdbType.charArray)
						len += ((char[])value).Length;
					else if (t.MdbType == MdbType.@object)
#if SERIALIZEBINARY
						len += MessagePackSerializer.Typeless.Serialize(value).Length;
#else
						len += JSONExtensions.SerializeObject(value).Length;
#endif
					else
						len += t.Size;
				}
				if (_ms.Position + len > limit)
					throw new OverflowException("Header size is larger than the buffer size specified in HeaderSpaceSize");
				_bw.Write(kvp.Key);
				if (isNull)
					_bw.Write((byte)MdbType.@null);
				else
					_dti[t.Type].WriteDelegate(_bw, value);
			}
			return this;
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
                dr["IsLong"] = c.DataType.IsNumeric(NumericTypesScope.Integral) ? true : false;
                dr["AllowDBNull"] = c.AllowDBNull;
                dr["IsReadOnly"] = false;
                dr["IsRowVersion"] = false;
                dr["IsUnique"] = false;
                dr["IsKey"] = false;
                dr["IsAutoIncrement"] = false;
                dr["BaseSchemaName"] = null;
                dr["BaseTableName"] = null;
                dr["BaseColumnName"] = _indexes[i];
            }
            return dt;
        }
		/// <summary>
		/// Get Closed state of <see cref="DataPackage"/>
		/// </summary>
        public bool IsClosed => (_br == null);
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
		/// Add a new row to <see cref="DataPackage"/>
		/// </summary>
		public DataPackage AddNew()
		{
			_dataRow = new Dictionary<string, object>(_colCount, StringComparer.CurrentCultureIgnoreCase);
			_rowPos = _ms.Length;
			return this;
		}
		/// <summary>
		/// <see cref="IDataReader.Read()"/> advance <see cref="DataPackage"/> to the next record
		/// </summary>
		/// <returns></returns>
		public bool Read()
		{
			_dataRow = new Dictionary<string, object>(_colCount, StringComparer.CurrentCultureIgnoreCase);
			if (_ms.Position >= _ms.Length - 1)
			{
				_rowPos = 0;
				return false;
			}

			_rowPos = _ms.Position;
			for (int i = 0; i < _colCount; i++)
			{
				MdbType t = (MdbType)_br.ReadByte();
				if (t == MdbType.@null)
					_dataRow.Add(_indexes[i], null);
				else
					_dataRow.Add(_indexes[i], _dti[t.Type()].ReadDelegate(_br));

			}
			return true;
		}
		/// <summary>
		/// <see cref="IDataReader.Read()"/> Not supported
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
		/// <summary>
		/// <see cref="IDisposable.Dispose(bool)"/>
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_bw != null) _bw.Dispose();
				_br.Dispose();
				_ms.Dispose();
				_ms = null;
				_br = null;
			}
		}
		/// <summary>
		/// Get count of columns (fields)
		/// </summary>
		public int FieldCount => _colCount;

		public bool GetBoolean(int i)=> (bool)_dataRow[_indexes[i]];

		public byte GetByte(int i) => (byte)_dataRow[_indexes[i]];

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i) => (char)_dataRow[_indexes[i]];

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i) => MdbTypeMap.GetTypeInfo(_colInfo[i].DataType).Name;

		public DateTime GetDateTime(int i) => (DateTime)_dataRow[_indexes[i]];

		public decimal GetDecimal(int i) => (decimal)_dataRow[_indexes[i]];

		public double GetDouble(int i) => (double)_dataRow[_indexes[i]];

		public Type GetFieldType(int i) => _colInfo[i].DataType;

		public float GetFloat(int i) => (float)_dataRow[_indexes[i]];

		public Guid GetGuid(int i) => (Guid)_dataRow[_indexes[i]];

		public short GetInt16(int i) => (Int16)_dataRow[_indexes[i]];

		public int GetInt32(int i) => (Int32)_dataRow[_indexes[i]];

		public long GetInt64(int i) => (Int64)_dataRow[_indexes[i]];

		public string GetName(int i) => _indexes[i];

		public int GetOrdinal(string name) => _indexes.IndexOf(name);

		public string GetString(int i) => (string)_dataRow[_indexes[i]];

		public object GetValue(int i) => _dataRow[_indexes[i]];

		public int GetValues(object[] values)
		{
			int i = 0;
			foreach (object value in _dataRow.Values)
			{
				values[i] = value;
				i++;
			}
			return i;
		}

		public JsonObject GetRowJSON()
			=> new JsonObject(_dataRow.Select(kvp => new KeyValuePair<string, JsonValue>(kvp.Key, new JsonValue(kvp.Value))));

        public bool IsDBNull(int i) => DBNull.Value.Equals(_dataRow[_indexes[i]]);

		public object this[string name]
		{
			get { return _dataRow[name]; }
			set { _dataRow[name] = value; }
		}

		public object this[int i]
		{
			get { return _dataRow[_indexes[i]]; }
			set { _dataRow[_indexes[i]] = value; }
		}

		public DataPackage SetValue(string name, object value)
		{
			_dataRow[name] = value;
			return this;
		}
		static void WriteColumnInfo(BinaryWriter bw, ColumnInfo ci)
		{
			if (_dti.ContainsKey(ci.DataType))
				bw.Write((byte)_dti[ci.DataType].ID);
			else
				bw.Write((byte)MdbType.byteArray);

			bw.Write(ci.ColumnSize);
			bw.Write(ci.AllowDBNull);
		}

		static void WriteByteArray(BinaryWriter bw, byte[] b)
		{
			int len = b.Length;
			bw.Write(len);
			if (len > 0) bw.Write(b);
		}

		static void WriteCharArray(BinaryWriter bw, char[] c)
		{
			int len = c.Length;
			bw.Write(len);
			if (len > 0) bw.Write(c);
		}

		static ColumnInfo ReadColumnInfo(BinaryReader br)
		{
			try
			{
				ColumnInfo ci = new ColumnInfo()
				{
					DataType = MdbTypeMap.GetType((MdbType)br.ReadByte()),
					ColumnSize = br.ReadInt32(),
					AllowDBNull = br.ReadBoolean()
				};
				return ci;
			}
			catch (Exception e)
			{
				throw new DataPackageMessageFormatException(e.Message);
			}			
		}

		static byte[] ReadByteArray(BinaryReader br)
		{
			int len = br.ReadInt32();
			if (len > 0) return br.ReadBytes(len);
			return new byte[0];
		}

		static char[] ReadCharArray(BinaryReader br)
		{
			int len = br.ReadInt32();
			if (len > 0) return br.ReadChars(len);
			return new char[0];
		}

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
			JsonObject j = new JsonObject();

			JsonObject headers = new JsonObject();
			_ms.Seek(_headerPos, SeekOrigin.Begin);
			int headCount = _br.ReadInt32();
			for (int i = 0; i < headCount; i++)
			{
				string key = _br.ReadString();
				MdbType t = (MdbType)_br.ReadByte();
				if (t == MdbType.@null)
					headers[key] = null;
				else
				{
					Type tp = t.Type();
					object v = _dti[tp].ReadDelegate(_br);
					headers[key] = new JsonValue(v);
				}
			}
			j["Headers"] = headers;

			JsonArray columns = new JsonArray();
			for (int i = 0; i < _indexes.Count; i++)
			{
				ColumnInfo ci = _colInfo[i];

				columns.Add(new JsonObject{ { "Name", _indexes[i] },
				{ "Type", MdbTypeMap.GetTypeInfo(ci.DataType).Name },
				{ "Size", ci.ColumnSize },
				{ "AllowNull", ci.AllowDBNull} });
			}

			JsonArray rows = new JsonArray();
			GoDataTop();
			for (; _ms.Position < _ms.Length - 1;)
			{
				JsonObject dr = new JsonObject();
				for (int i = 0; i < _colCount; i++)
				{
					MdbType t = (MdbType)_br.ReadByte();
					string colName = _indexes[i];
					if (t == MdbType.@null)
						dr[colName] = null;
					else
					{
						Type tp = MdbTypeMap.GetType(t);
						object v = _dti[tp].ReadDelegate(_br);
						dr[colName] = new JsonValue(v);
					}
				}
				rows.Add(dr);
			}
			//j["TableName"] = _tableName;
			//j["BinaryMode"] = (byte)_bm;
			j["HeaderSize"] = _headerSpaceSize;
			j["Columns"] = columns;
			j["Rows"] = rows;
			return j.ToString();
		}

		public DataTable ToDataTable()
		{
			var dt = new DataTable();
			for (int i = 0; i < _indexes.Count; i++)
			{
				ColumnInfo ci = _colInfo[i];
				DataColumn dc = new DataColumn(_indexes[i], ci.DataType);
				if (ci.DataType == typeof(string))
					dc.MaxLength = ci.ColumnSize;
				dc.AllowDBNull = ci.AllowDBNull;
				dt.Columns.Add(dc);
			}
			GoDataTop();
			for (; _ms.Position < _ms.Length - 1;)
			{
				DataRow dr = dt.NewRow();
				for (int i = 0; i < _colCount; i++)
				{
					MdbType t = (MdbType)_br.ReadByte();
					if (t == MdbType.@null)
						dr[i] = DBNull.Value;
					else
						dr[i] = _dti[MdbTypeMap.GetType(t)].ReadDelegate(_br);
				}
				dt.Rows.Add(dr);
			}
			GoDataTop();
			return dt;
		}
		public static DataPackage Parse(int headerSpaceSize, string jsonString)
		{
			JsonObject j = (JsonObject)new JsonReader(ref jsonString).Read();
			DataPackage ts = new DataPackage(headerSpaceSize, 
				j.Select(kvp => kvp.Key).ToArray<string>(),
				j.Select(kvp => kvp.Value?.GetValue()).ToArray<object>());
			ts.Update();
			ts.GoDataTop();
			return ts;
		}

		public static DataPackage Parse(string jsonString)
		{
			JsonObject j = (JsonObject)new JsonReader(ref jsonString).Read();
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
				foreach (var jo in r)
				{
					ts[jo.Key] = jo.Value?.GetValue();
				}
				ts.Update();
			}
			ts.GoDataTop();
			return ts;
		}
	}
	public class DataPackageMessageFormatException: Exception
	{
		public DataPackageMessageFormatException() : base() { }
		public DataPackageMessageFormatException(string message) : base(message) { }
	}

}
