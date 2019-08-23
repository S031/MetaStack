using S031.MetaStack.Json;
using System;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public sealed class JMXIndex
    {
		public JMXIndex()
		{
			KeyMembers = new List<JMXKeyMember>();
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
		public List<JMXKeyMember> KeyMembers { get; }
		public void AddKeyMember(params JMXKeyMember[] members)
		{
			foreach (var m in members)
				KeyMembers.Add(m);
		}
		public void AddKeyMember(params string[] attribNames)
		{
			int position = KeyMembers.Count + 1;
			foreach (var n in attribNames)
			{
				KeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
				position++;
			}
		}
		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			writer.WriteStartObject();
			ToStringRaw(writer);
			writer.WriteEndObject();
			return writer.ToString();
		}
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("ID", ID);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("IndexName", IndexName);
			writer.WriteProperty("IsUnique", IsUnique);
			writer.WriteProperty("ClusteredOption", ClusteredOption);
			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in KeyMembers)
				item.ToStringRaw(writer);
			writer.WriteEndArray();
		}
		internal JMXIndex(JsonObject o)
		{
			IndexName = o.GetStringOrDefault("IndexName");
			ClusteredOption = o.GetIntOrDefault("ClusteredOption");
			IsUnique = o.GetBoolOrDefault("IsUnique");
			ID = o.GetIntOrDefault("ID");
			UID = o.GetGuidOrDefault("UID");

			KeyMembers = new List<JMXKeyMember>();
			if (o.TryGetValue("KeyMembers", out JsonArray a))
				foreach (JsonObject m in a)
					KeyMembers.Add(JMXKeyMember.ReadFrom(m));

		}
		internal static JMXIndex ReadFrom(JsonObject o)
			=> new JMXIndex(o);
	}
}
