using System;
using System.Collections.Generic;

namespace S031.MetaStack.Data
{
	/// <summary>
	/// Inherited from <see cref="TypeCode"/> 
	/// </summary>
	public enum MdbType : byte
	{
		@none = TypeCode.Empty,
		@object = TypeCode.Object,
		@null = TypeCode.DBNull,
		@bool = TypeCode.Boolean,
		@char = TypeCode.Char,
		@sbyte = TypeCode.SByte,
		@byte = TypeCode.Byte,
		@short = TypeCode.Int16,
		@ushort = TypeCode.UInt16,
		@int = TypeCode.Int32,
		@uint = TypeCode.UInt32,
		@long = TypeCode.Int64,
		@ulong = TypeCode.UInt64,
		@float = TypeCode.Single,
		@double = TypeCode.Double,
		@decimal = TypeCode.Decimal,
		dateTime = TypeCode.DateTime,
		@guid = TypeCode.String - 1,
		@string = TypeCode.String,
		byteArray = TypeCode.String + 1,
	}

	/// <summary>
	/// ref && readonly
	/// </summary>
	public struct MdbTypeInfo
	{
		public MdbType MdbType;
		public Type Type;
		public string Name;
		public int Size;
		public int Scale;
		public int Precision;
		public bool FixedSize;
		public bool NullIfEmpty;
	}

	public static class MdbTypeMap
	{
		static readonly MdbTypeInfo[] _listTypesInfo = new MdbTypeInfo[]
		{
			new MdbTypeInfo() { MdbType = MdbType.none, Type = null, Name = "none", Size = sizeof(byte), Scale = 0, Precision = 1, FixedSize = true, NullIfEmpty = false},
			new MdbTypeInfo() { MdbType = MdbType.@object, Type = typeof(object), Name = "object", Size = sizeof(int), Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@null, Type = typeof(DBNull), Name = "null", Size = sizeof(byte), Scale = 0, Precision = 1, FixedSize = true, NullIfEmpty = false},
			new MdbTypeInfo() { MdbType = MdbType.@bool, Type = typeof(bool), Name = "bool", Size = sizeof(bool), Scale = 0, Precision = 1, FixedSize = true, NullIfEmpty = false},
			new MdbTypeInfo() { MdbType = MdbType.@char, Type = typeof(char), Name = "char", Size = sizeof(char), Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@sbyte, Type = typeof(sbyte), Name = "sbyte", Size = sizeof(sbyte), Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@byte, Type = typeof(byte), Name = "byte", Size = sizeof(byte), Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@short, Type = typeof(short), Name = "short", Size = sizeof(short), Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@ushort, Type = typeof(ushort), Name = "ushort", Size = sizeof(ushort), Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@int, Type = typeof(int), Name = "int", Size = sizeof(int), Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@uint, Type = typeof(uint), Name = "uint", Size = sizeof(uint), Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "long", Size = sizeof(long), Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@ulong, Type = typeof(ulong), Name = "ulong", Size = sizeof(ulong), Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@float, Type = typeof(float), Name = "float", Size = sizeof(float), Scale = 0, Precision = 53, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@double, Type = typeof(double), Name = "double", Size = sizeof(double), Scale = 0, Precision = 24, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "decimal", Size = sizeof(decimal), Scale = 0, Precision = 38, FixedSize = false, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime", Size = sizeof(long), Scale = 3, Precision = 23, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.guid, Type = typeof(Guid), Name = "guid", Size = 16, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "string", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true},
			new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "byte[]", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true}
		};

		public static MdbTypeInfo GetTypeInfo(this MdbType t)
			=> _listTypesInfo[(int)t];

		public static MdbTypeInfo GetTypeInfo(Type t)
		{
			TypeCode tcode = System.Type.GetTypeCode(t);

			if (tcode == TypeCode.Object)
			{
				if (t == typeof(byte[]))
					return GetTypeInfo(MdbType.byteArray);
				else if (t == typeof(Guid))
					return GetTypeInfo(MdbType.guid);
				else
					return GetTypeInfo(MdbType.@object);
			}
			return _listTypesInfo[(int)tcode];
		}

		public static Type GetType(MdbType t)
			=> _listTypesInfo[(int)t]
			.Type;

		public static Type GetType(Type t)
			=> GetTypeInfo(t)
			.Type;

		public static Type GetType(string typeName)
			=> GetType(typeName, typeof(object));

		public static Type GetType(string typeName, Type defaultType)
		{
			return Enum.TryParse<MdbType>(typeName, true, out MdbType t)
				? _listTypesInfo[(int)t].Type
				: defaultType;
		}

		public static Type Type(this MdbType mdbType)
			=> GetType(mdbType);
	}
}
