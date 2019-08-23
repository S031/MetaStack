using S031.MetaStack.Json;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public sealed class JMXPrimaryKey
	{
		public JMXPrimaryKey()
		{
			KeyMembers = new List<JMXKeyMember>();
		}
		public JMXPrimaryKey(string keyName, params string[] keyMemberNames):this()
		{
			KeyName = keyName;
			AddKeyMember(keyMemberNames);
		}
		public int ID { get; set; }
		public int Handle { get => ID; set => ID = value; }
		public string KeyName { get; set; }
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
			writer.WriteProperty("Handle", Handle);
			writer.WriteProperty("KeyName", KeyName);
			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in KeyMembers)
				item.ToStringRaw(writer);
			writer.WriteEndArray();
		}
		internal JMXPrimaryKey(JsonObject o)
		{
			KeyName = o.GetStringOrDefault("KeyName");
			ID = o.GetIntOrDefault("ID");

			KeyMembers = new List<JMXKeyMember>();
			if (o.TryGetValue("KeyMembers", out JsonArray a))
				foreach (JsonObject m in a)
					KeyMembers.Add(JMXKeyMember.ReadFrom(m));
		}
		internal static JMXPrimaryKey ReadFrom(JsonObject o)
			=> new JMXPrimaryKey(o);
	}
}
