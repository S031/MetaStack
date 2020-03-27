using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteTypeMapping : IJMXTypeMapping
	{
		private static readonly ReadOnlyCache<MdbType, string> _typeMap = new ReadOnlyCache<MdbType, string>
		(
			(MdbType.@null, "int"),
			(MdbType.@bool, "bit"),
			(MdbType.@char, "tinyint"),
			(MdbType.@byte, "tinyint"),
			(MdbType.@sbyte, "tinyint"),
			(MdbType.@short, "smallint"),
			(MdbType.@ushort, "smallint"),
			(MdbType.@int, "int"),
			(MdbType.@uint, "int"),
			(MdbType.@long, "bigint"),
			(MdbType.@ulong, "bigint"),
			(MdbType.@float, "float;real"),
			(MdbType.@double, "float;real"),
			(MdbType.@decimal, "decimal;money;numeric;smallmoney"),
			(MdbType.dateTime, "date;datetime;datetime2;smalldatetime;time"),
			(MdbType.@string, "varchar;nvarchar;text;ntext;char;nchar;xml"),
			(MdbType.byteArray, "binary;varbinary;image;rowversion;timestamp;sql_variant"),
			//(MdbType.charArray, "char;nchar"),
			(MdbType.guid, "uniqueidentifier"),
			(MdbType.@object, "varchar")
		);

		private static readonly ReadOnlyCache<string, MdbTypeInfo> _typeInfo = new ReadOnlyCache<string, MdbTypeInfo>(StringComparer.OrdinalIgnoreCase,
			("bit", new MdbTypeInfo() { MdbType = MdbType.@bool, Type = typeof(bool), Name = "bit", Size = 1, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = false }),
			("tinyint", new MdbTypeInfo() { MdbType = MdbType.@byte, Type = typeof(byte), Name = "tinyint", Size = 1, Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true }),
			("smallint", new MdbTypeInfo() { MdbType = MdbType.@short, Type = typeof(short), Name = "smallint", Size = 2, Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true }),
			("int", new MdbTypeInfo() { MdbType = MdbType.@int, Type = typeof(int), Name = "int", Size = 4, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true }),
			("bigint", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "bigint", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true }),
			("real", new MdbTypeInfo() { MdbType = MdbType.@double, Type = typeof(double), Name = "real", Size = 4, Scale = 0, Precision = 24, FixedSize = true, NullIfEmpty = true }),
			("float", new MdbTypeInfo() { MdbType = MdbType.@float, Type = typeof(float), Name = "float", Size = 8, Scale = 0, Precision = 53, FixedSize = true, NullIfEmpty = true }),
			("time", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "time", Size = 5, Scale = 7, Precision = 16, FixedSize = true, NullIfEmpty = true }),
			("date", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "date", Size = 3, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true }),
			("datetime", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime", Size = 8, Scale = 3, Precision = 23, FixedSize = true, NullIfEmpty = true }),
			("datetime2", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime2", Size = 8, Scale = 7, Precision = 27, FixedSize = true, NullIfEmpty = true }),
			("smalldatetime", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "smalldatetime", Size = 4, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true }),
			("timestamp", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "timestamp", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true }),
			("smallmoney", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "smallmoney", Size = 4, Scale = 4, Precision = 10, FixedSize = true, NullIfEmpty = true }),
			("money", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "money", Size = 8, Scale = 4, Precision = 19, FixedSize = true, NullIfEmpty = true }),
			("numeric", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "numeric", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true }),
			("decimal", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "decimal", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true }),
			("text", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "text", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("ntext", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "ntext", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("varchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "varchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("nvarchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nvarchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("char", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "char", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("nchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("xml", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "xml", Size = -1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("binary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "binary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("varbinary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "varbinary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("image", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "image", Size = 16, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true }),
			("uniqueidentifier", new MdbTypeInfo() { MdbType = MdbType.guid, Type = typeof(Guid), Name = "uniqueidentifier", Size = 16, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true }),
			("sql_variant", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "sql_variant", Size = 8016, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true })
		);

		public IEnumerable<string> GetVariableLenghtDataTypes() =>
			new string[]
		{
			"char",
			"nchar",
			"varchar",
			"nvarchar",
			"varbinary"
		};

		public IReadOnlyDictionary<string, MdbTypeInfo> GetServerTypeMap() => _typeInfo;

		public IReadOnlyDictionary<MdbType, string> GetTypeMap() => _typeMap;
	}
}
