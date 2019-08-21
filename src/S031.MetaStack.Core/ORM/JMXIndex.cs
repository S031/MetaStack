using S031.MetaStack.Json;
using System;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXIndex
    {
		private List<JMXKeyMember> _keyMembers;
		public JMXIndex()
		{
			_keyMembers = new List<JMXKeyMember>();
		}
		public JMXIndex(string indexName, params string[] keyMemberNamnes):this()
		{
			IndexName = indexName;
			AddKeyMember(keyMemberNamnes);
		}
		public int ID { get; set; }
		public Guid UID { get; set; }
		public string IndexName { get; set; }
		public bool IsUnique { get; set; }
		public int ClusteredOption { get; set; }
		public List<JMXKeyMember> KeyMembers { get => _keyMembers; }
		public void AddKeyMember(params JMXKeyMember[] members)
		{
			foreach (var m in members)
				_keyMembers.Add(m);
		}
		public void AddKeyMember(params string[] attribNames)
		{
			int position = _keyMembers.Count + 1;
			foreach (var n in attribNames)
			{
				_keyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
				position++;
			}
		}
		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("ID", ID);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("IndexName", IndexName);
			writer.WriteProperty("IsUnique", IsUnique);
			writer.WriteProperty("ClusteredOption", ClusteredOption);
			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in _keyMembers)
				item.ToStringRaw(writer);
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		internal static JMXIndex ReadFrom(JsonObject o)
		{
			var fk = new JMXIndex()
			{
				IndexName = o["IndexName"],
				ClusteredOption = o.GetIntOrDefault("ClusteredOption"),
				IsUnique = o.GetBoolOrDefault("IsUnique"),
				ID = o.GetIntOrDefault("ID"),
				UID = o.GetGuidOrDefault("UID")
			};

			if (o.TryGetValue("KeyMembers", out JsonArray a))
			{
				foreach (JsonObject m in a)
				{
					fk.KeyMembers.Add(new JMXKeyMember()
					{
						FieldName = m["FieldName"],
						Position = m.GetIntOrDefault("Position"),
						IsIncluded = m.GetBoolOrDefault("IsIncluded"),
						IsDescending = m.GetBoolOrDefault("IsDescending")
					});
				}
			}
			return fk;
		}
	}
}
