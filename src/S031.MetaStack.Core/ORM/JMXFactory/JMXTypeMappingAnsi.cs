using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM
{
	public class JMXTypeMappingAnsi : IJMXTypeMapping
	{
		/*
		CHARACTER
		CHARACTER VARYING(or VARCHAR)
		CHARACTER LARGE OBJECT
		NCHAR
		NCHAR VARYING
		BINARY
		BINARY VARYING
		BINARY LARGE OBJECT
		NUMERIC
		DECIMAL
		SMALLINT
		INTEGER
		BIGINT
		FLOAT
		REAL
		DOUBLE PRECISION
		BOOLEAN
		DATE
		TIME
		TIMESTAMP
		INTERVAL
		*/
		private static readonly Dictionary<MdbType, string> _typeMap = new Dictionary<MdbType, string>()
		{
			{MdbType.@null, "integer" },
			{MdbType.@bool, "boolean" },
			{MdbType.@char, "smallint" },
			{MdbType.@byte, "smallint" },
			{MdbType.@sbyte, "smallint" },
			{MdbType.@short, "smallint" },
			{MdbType.@ushort, "smallint" },
			{MdbType.@int, "integer" },
			{MdbType.@uint, "integer" },
			{MdbType.@long, "bigint" },
			{MdbType.@ulong, "bigint" },
			{MdbType.@float, "float;real" },
			{MdbType.@double, "float;real" },
			{MdbType.@decimal, "decimal;numeric" },
			{MdbType.dateTime, "date;time;timestamp" },
			{MdbType.@string, "varchar;nvarchar;char;nchar" },
			{MdbType.byteArray, "binary;varbinary" },
			{MdbType.charArray, "binary" },
			{MdbType.guid, "binary" },
			{MdbType.@object, "varchar" }
		};

		private static readonly Dictionary<string, MdbTypeInfo> _typeInfo = new Dictionary<string, MdbTypeInfo>(StringComparer.CurrentCultureIgnoreCase)
		{
			{ "boolean", new MdbTypeInfo() { MdbType = MdbType.@bool, Type = typeof(bool), Name = "bit", Size = 1, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = false} },
			{ "smallint", new MdbTypeInfo() { MdbType = MdbType.@short, Type = typeof(short), Name = "smallint", Size = 2, Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true} },
			{ "integer", new MdbTypeInfo() { MdbType = MdbType.@int, Type = typeof(int), Name = "int", Size = 4, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ "bigint", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "bigint", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ "real", new MdbTypeInfo() { MdbType = MdbType.@double, Type = typeof(double), Name = "real", Size = 4, Scale = 0, Precision = 24, FixedSize = true, NullIfEmpty = true} },
			{ "float", new MdbTypeInfo() { MdbType = MdbType.@float, Type = typeof(float), Name = "float", Size = 8, Scale = 0, Precision = 53, FixedSize = true, NullIfEmpty = true} },
			{ "time", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "time", Size = 5, Scale = 7, Precision = 16, FixedSize = true, NullIfEmpty = true} },
			{ "date", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "date", Size = 3, Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ "datetime", new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime", Size = 8, Scale = 3, Precision = 23, FixedSize = true, NullIfEmpty = true} },
			{ "timestamp", new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "timestamp", Size = 8, Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ "numeric", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "numeric", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true} },
			{ "decimal", new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "decimal", Size = 9, Scale = 0, Precision = 18, FixedSize = false, NullIfEmpty = true} },
			{ "varchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "varchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "nvarchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nvarchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "char", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "char", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "nchar", new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "nchar", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "binary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "binary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "varbinary", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "varbinary", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ "sql_variant", new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "sql_variant", Size = 8016, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
		};

		public IDictionary<string, MdbTypeInfo> GetServerTypeMap() => _typeInfo;

		public IDictionary<MdbType, string> GetTypeMap() => _typeMap;

		public IEnumerable<string> GetVariableLenghtDataTypes()
		{
			yield return "char";
			yield return "nchar";
			yield return "varchar";
			yield return "nvarchar";
			yield return "varying";
		}
	}
}
