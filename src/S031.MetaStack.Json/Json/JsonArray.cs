using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace S031.MetaStack.Json
{
	public class JsonArray : JsonValue, IList<JsonValue>
	{
		private readonly List<JsonValue> _list;

		public JsonArray(params JsonValue[] items)
		{
			_list = new List<JsonValue>();
			AddRange(items);
		}

		public JsonArray(IEnumerable<JsonValue> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			_list = new List<JsonValue>(items);
		}

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public override JsonValue this[int index]
		{
			get => _list[index];
			set => _list[index] = value;
		}

		public override JsonType JsonType => JsonType.Array;

		public void Add(JsonValue item)
		{
			_list.Add(item);
		}

		public void AddRange(IEnumerable<JsonValue> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			_list.AddRange(items);
		}

		public void AddRange(params JsonValue[] items)
		{
			if (items != null)
			{
				_list.AddRange(items);
			}
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(JsonValue item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(JsonValue[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public int IndexOf(JsonValue item)
		{
			return _list.IndexOf(item);
		}

		public void Insert(int index, JsonValue item)
		{
			_list.Insert(index, item);
		}

		public bool Remove(JsonValue item)
		{
			return _list.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public override string ToString()
			=> ToString(Formatting.None);

		public string ToString(Formatting formatting)
		{
			JsonWriter w = new JsonWriter(formatting);
			WriteRaw(w);
			return w.ToString();
		}

		public void WriteRaw(JsonWriter writer)
		{
			writer.WriteStartArray();
			foreach (JsonValue v in this)
			{
				if (v == null)
					writer.WriteNull();
				else
					writer.WriteValue(v);
			}
			writer.WriteEndArray();
		}

		public IEnumerator<JsonValue> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static JsonArray Parse(string source)
			=> (JsonArray)new JsonReader(source).Read();

		public static bool TryParse(string source, out JsonArray array)
		{
			try
			{
				array = (JsonArray)new JsonReader(source).Read();
				return true;
			}
			catch
			{
				array = null;
				return false;
			}
		}

		/// <summary>
		/// Fast simpale test string on json format, first and last characters are '[' and ']'
		/// </summary>
		/// <param name="stringForTest"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static new bool Test(string stringForTest)
			=> TestStringOnJsonFormat(stringForTest, new char[] {'['}, new char[] { ']' });
	}
}