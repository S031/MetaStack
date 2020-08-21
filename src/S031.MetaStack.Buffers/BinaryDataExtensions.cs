using System;

namespace S031.MetaStack.Buffers
{
	public static class BinaryDataExtensions
	{
		public static ExportedDataTypes GetExportedType(this Type type)
			=> BinaryDataWriter.GetExportedType(type);

		public static ExportedDataTypes GetExportedType(this object value)
			=> BinaryDataWriter.GetExportedType(value.GetType());
	}
}
