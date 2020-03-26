using S031.MetaStack.Json;
using System.Collections.Generic;

namespace S031.MetaStack.ORM
{
	public sealed class JMXPrimaryKey: JsonSerializible
	{
		public JMXPrimaryKey(string keyName, params string[] keyMemberNames):base(null)
		{
			KeyName = keyName;
			AddKeyMember(keyMemberNames);
		}
		public int ID { get; set; }
		public int Handle { get => ID; set => ID = value; }
		public string KeyName { get; set; }
		public List<JMXKeyMember> KeyMembers { get; } = new List<JMXKeyMember>();
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
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }
        internal JMXPrimaryKey(JsonObject o)
            :base(o)
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

        public override void ToJson(JsonWriter writer)
        {
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
        {
			writer.WriteProperty("ID", ID);
			writer.WriteProperty("Handle", Handle);
			writer.WriteProperty("KeyName", KeyName);
			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in KeyMembers)
				item.ToJson(writer);
			writer.WriteEndArray();
        }
    }
}
