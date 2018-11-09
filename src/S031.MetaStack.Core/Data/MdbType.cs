using System;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.Data
#else
namespace S031.MetaStack.WinForms.Data
#endif
{
	public enum MdbType : byte
	{
		@null,
		@bool,
		@char,
		@byte,
		@sbyte,
		@short,
		@ushort,
		@int,
		@uint,
		@long,
		@ulong,
		@float,
		@double,
		@decimal,
		dateTime,
		@string,
		byteArray,
		charArray,
		guid,
		@object
	}

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
		//Порядок в cтрогом сответствие MdbType, object - последний
		static readonly List<Type> _listTypes = new List<Type>()
		{
			null,
			typeof(bool),
			typeof(char),
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(DateTime),
			typeof(string),
			typeof(byte[]),
			typeof(char[]),
			typeof(Guid),
			typeof(object)
		};

		//Reference types size default 255
		static readonly Dictionary<Type, MdbTypeInfo> _listTypesInfo = new Dictionary<Type, MdbTypeInfo>()
		{
			{ typeof(bool), new MdbTypeInfo() { MdbType = MdbType.@bool, Type = typeof(bool), Name = "bool", Size = sizeof(bool), Scale = 0, Precision = 1, FixedSize = true, NullIfEmpty = false} },
			{ typeof(char), new MdbTypeInfo() { MdbType = MdbType.@char, Type = typeof(char), Name = "char", Size = sizeof(char), Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true} },
			{ typeof(byte), new MdbTypeInfo() { MdbType = MdbType.@byte, Type = typeof(byte), Name = "byte", Size = sizeof(byte), Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true} },
			{ typeof(sbyte), new MdbTypeInfo() { MdbType = MdbType.@sbyte, Type = typeof(sbyte), Name = "sbyte", Size = sizeof(sbyte), Scale = 0, Precision = 3, FixedSize = true, NullIfEmpty = true} },
			{ typeof(short), new MdbTypeInfo() { MdbType = MdbType.@short, Type = typeof(short), Name = "short", Size = sizeof(short), Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true} },
			{ typeof(ushort), new MdbTypeInfo() { MdbType = MdbType.@ushort, Type = typeof(ushort), Name = "ushort", Size = sizeof(ushort), Scale = 0, Precision = 5, FixedSize = true, NullIfEmpty = true} },
			{ typeof(int), new MdbTypeInfo() { MdbType = MdbType.@int, Type = typeof(int), Name = "int", Size = sizeof(int), Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ typeof(uint), new MdbTypeInfo() { MdbType = MdbType.@uint, Type = typeof(uint), Name = "uint", Size = sizeof(uint), Scale = 0, Precision = 10, FixedSize = true, NullIfEmpty = true} },
			{ typeof(long), new MdbTypeInfo() { MdbType = MdbType.@long, Type = typeof(long), Name = "long", Size = sizeof(long), Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ typeof(ulong), new MdbTypeInfo() { MdbType = MdbType.@ulong, Type = typeof(ulong), Name = "ulong", Size = sizeof(ulong), Scale = 0, Precision = 19, FixedSize = true, NullIfEmpty = true} },
			{ typeof(float), new MdbTypeInfo() { MdbType = MdbType.@float, Type = typeof(float), Name = "float", Size = sizeof(float), Scale = 0, Precision = 53, FixedSize = true, NullIfEmpty = true} },
			{ typeof(double), new MdbTypeInfo() { MdbType = MdbType.@double, Type = typeof(double), Name = "double", Size = sizeof(double), Scale = 0, Precision = 24, FixedSize = true, NullIfEmpty = true} },
			{ typeof(decimal), new MdbTypeInfo() { MdbType = MdbType.@decimal, Type = typeof(decimal), Name = "decimal", Size = sizeof(decimal), Scale = 0, Precision = 38, FixedSize = false, NullIfEmpty = true} },
			{ typeof(DateTime), new MdbTypeInfo() { MdbType = MdbType.dateTime, Type = typeof(DateTime), Name = "datetime", Size = sizeof(long), Scale = 3, Precision = 23, FixedSize = true, NullIfEmpty = true} },
			{ typeof(string), new MdbTypeInfo() { MdbType = MdbType.@string, Type = typeof(string), Name = "string", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ typeof(byte[]), new MdbTypeInfo() { MdbType = MdbType.byteArray, Type = typeof(byte[]), Name = "byte[]", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ typeof(char[]), new MdbTypeInfo() { MdbType = MdbType.charArray, Type = typeof(char[]), Name = "char[]", Size = 1, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} },
			{ typeof(Guid), new MdbTypeInfo() { MdbType = MdbType.guid, Type = typeof(Guid), Name = "guid", Size = 16, Scale = 0, Precision = 0, FixedSize = true, NullIfEmpty = true} },
			{ typeof(object), new MdbTypeInfo() { MdbType = MdbType.@object, Type = typeof(object), Name = "object", Size = 10, Scale = 0, Precision = 0, FixedSize = false, NullIfEmpty = true} }
		};
		public static MdbTypeInfo GetTypeInfo(this MdbType t) => _listTypesInfo[_listTypes[(int)t]];
		public static MdbTypeInfo GetTypeInfo(Type t)
		{
			if (_listTypesInfo.TryGetValue(t, out MdbTypeInfo v))
				return v;
			return _listTypesInfo[typeof(object)];
		}
		public static Type GetType(MdbType t) => _listTypes[(int)t];
		public static Type GetType(Type t)
		{
			if (_listTypesInfo.ContainsKey(t))
				return t;
			return typeof(object);
		}
		public static Type GetType(string typeName) => GetType(typeName, typeof(object));
		public static Type GetType(string typeName, Type defaultType)
		{
			if (Enum.TryParse<MdbType>(typeName, true, out MdbType t))
				return _listTypes[(int)t];
			return defaultType;
		}

		public static Type Type(this MdbType mdbType) => GetType(mdbType);
	}
}
