using S031.MetaStack.Json;
using System;
using System.Collections.Generic;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public sealed class JMXIndex: JsonSerializible
    {
        public JMXIndex(string indexName, params string[] keyMemberNamnes) : base(null)
        {
            IndexName = indexName;
            AddKeyMember(keyMemberNamnes);
        }
		public int ID { get; set; }
		public Guid UID { get; set; }
		public string IndexName { get; set; }
		public bool IsUnique { get; set; }
		public int ClusteredOption { get; set; }
		public List<JMXKeyMember> KeyMembers { get; }= new List<JMXKeyMember>();
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

        internal JMXIndex(JsonValue value) : base(value)
        {
            JsonObject o = value as JsonObject;
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
		internal static JMXIndex ReadFrom(JsonValue  o)
			=> new JMXIndex(o);

        public override void ToJson(JsonWriter writer)
        {
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
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
    }
}
