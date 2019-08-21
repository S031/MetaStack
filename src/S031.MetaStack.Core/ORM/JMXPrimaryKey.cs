using S031.MetaStack.Json;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXPrimaryKey
	{
		private readonly List<JMXKeyMember> _keyMembers;

		public JMXPrimaryKey()
		{
			_keyMembers = new List<JMXKeyMember>();
		}
		public JMXPrimaryKey(string keyName, params string[] keyMemberNames):this()
		{
			KeyName = keyName;
			AddKeyMember(keyMemberNames);
		}
		public int ID { get; set; }
		public int Handle { get => ID; set => ID = value; }
		public string KeyName { get; set; }
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
			writer.WriteProperty("Handle", Handle);
			writer.WriteProperty("KeyName", KeyName);
			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in _keyMembers)
				item.ToStringRaw(writer);
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		internal static JMXPrimaryKey ReadFrom(JsonObject o)
		{
			var pk = new JMXPrimaryKey
			{
				KeyName = o["KeyName"],
				ID = o.GetIntOrDefault("ID")
			};
			var km = (o["KeyMembers"] as JsonArray);
			foreach (JsonObject m in km)
			{
				pk.KeyMembers.Add(new JMXKeyMember()
				{
					FieldName = m["FieldName"],
					Position = m.GetIntOrDefault("Position"),
					IsIncluded = m.GetBoolOrDefault("IsIncluded"),
					IsDescending = m.GetBoolOrDefault("IsDescending")
				});
			}
			return pk;
		}
	}
}
