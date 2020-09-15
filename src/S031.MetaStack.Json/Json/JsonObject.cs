using S031.MetaStack.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonPair = System.Collections.Generic.KeyValuePair<string, S031.MetaStack.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, S031.MetaStack.Json.JsonValue>>;

namespace S031.MetaStack.Json
{
	public class JsonObject : JsonValue, IDictionary<string, JsonValue>, ICollection<JsonPair>
	{
		// Use SortedDictionary to make result of ToString() deterministic
		private readonly MapTable<string, JsonValue> _map;

		public JsonObject(params JsonPair[] items)
		{
			_map = new MapTable<string, JsonValue>();

			if (items != null)
			{
				AddRange(items);
			}
		}

		public JsonObject(JsonPairEnumerable items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			_map = new MapTable<string, JsonValue>();
			AddRange(items);
		}

		public int Count => _map.Count;

		public IEnumerator<JsonPair> GetEnumerator() => _map.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _map.GetEnumerator();

		public override JsonValue this[string key]
		{
			get => _map[key];
			set => _map[key] = value;
		}
		public override JsonValue this[int index] => JsonType == JsonType.Array ? ((this as JsonValue) as JsonArray)[index] : throw new NotSupportedException();

		public override JsonType JsonType => JsonType.Object;

		public ICollection<string> Keys => _map.Keys;

		public ICollection<JsonValue> Values => _map.Values;

		public void Add(string key, JsonValue value) => _map.Add(key, value);

		public void Add(JsonPair pair) => Add(pair.Key, pair.Value);

		public void AddRange(JsonPairEnumerable items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			foreach (JsonPair pair in items)
			{
				_map.Add(pair.Key, pair.Value);
			}
		}

		public void AddRange(params JsonPair[] items) => AddRange((JsonPairEnumerable)items);

		public void Clear() => _map.Clear();

		public bool Contains(JsonPair item) => (_map as ICollection<JsonPair>).Contains(item);

		public bool Remove(JsonPair item) => (_map as ICollection<JsonPair>).Remove(item);

		public bool ContainsKey(string key) => _map.ContainsKey(key);

		public void CopyTo(JsonPair[] array, int arrayIndex) => (_map as ICollection<JsonPair>).CopyTo(array, arrayIndex);

		public bool Remove(string key) => _map.Remove(key);

		public bool IsReadOnly => false;

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
			writer.WriteStartObject();
			foreach (JsonPair pair in this)
			{
				//Use options for escape property names
				writer.WritePropertyName(pair.Key, false); 
				if (pair.Value == null)
					writer.WriteNull();
				else
					writer.WriteValue(pair.Value);
			}
			writer.WriteEndObject();
		}

		public bool TryGetValue(string key, out JsonValue value) => _map.TryGetValue(key, out value);

		public bool TryGetValue(string key, out JsonObject value)
		{
			if (_map.TryGetValue(key, out JsonValue v))
			{
				value = v as JsonObject;
				return true;
			}
			value = default;
			return false;
		}

		public bool TryGetValue(string key, out JsonArray value)
		{
			if (_map.TryGetValue(key, out JsonValue v))
			{
				value = v as JsonArray;
				return true;
			}
			value = default;
			return false;
		}

		public JsonPair GetPair()
			=> _map.FirstOrDefault();

		//public static implicit operator JsonObject(string value) => (JsonObject)new JsonReader(value).Read();
		
		//public static implicit operator string(JsonObject value) => value.ToString();
	}
}