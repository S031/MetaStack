using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Json.Writer
{
	public enum Formatting
	{
		None = 0,
		Indented = 1
	}
	public readonly ref struct JsonWriter
	{
		private const int default_capacity = 128;

		private readonly StringBuilder _sb;
		private readonly Formatting _formatting;

		public JsonWriter(Formatting formatting)
		{
			_sb = new StringBuilder(default_capacity);
			_formatting = formatting;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteChar(char value)
		{
			_sb.Append(value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteStartArray()
			=> WriteChar('[');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteEndArray()
			=> WriteChar(']');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteStartObject()
			=> WriteChar('{');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteEndObject()
			=> WriteChar('}');
	}
}
