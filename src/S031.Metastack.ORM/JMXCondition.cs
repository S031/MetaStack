using System;
using System.IO;
using System.Text;
using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public enum JMXConditionTypes
	{
		none,
		Where,
		OrderBy,
		GroupBy,
		Havind,
		Join
	}
    public class JMXCondition : JsonSerializible
    {

        public JMXCondition(JMXConditionTypes conditionType, string definition) : base(null)
        {
            ConditionType = conditionType;
            Definition = definition;
        }

        public string Definition { get; }

        public JMXConditionTypes ConditionType { get; }


        public static bool operator ==(JMXCondition m1, JMXCondition m2)
        {
            return (m1.Definition == m2.Definition &&
                m1.ConditionType == m2.ConditionType);
        }

        public static bool operator !=(JMXCondition m1, JMXCondition m2)
        {
            return (m1.Definition != m2.Definition ||
                m1.ConditionType != m2.ConditionType);
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return new { Definition, ConditionType }.GetHashCode();
        }

        public bool IsEmpty() => string.IsNullOrEmpty(Definition);

        public override string ToString()
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }

        public JMXCondition(JsonValue value) : base(value)
        {
            JsonObject o = value as JsonObject;
            ConditionType = o.GetEnum<JMXConditionTypes>("ConditionType");
            Definition = o["Definition"];
        }

        public override void ToJson(JsonWriter writer)
        {
            writer.WriteStartObject();
            ToJsonRaw(writer);
            writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
        {
            writer.WriteProperty("ConditionType", ConditionType.ToString());
            writer.WriteProperty("Definition", Definition);
        }

        internal static JMXCondition ReadFrom(JsonObject o)
            => new JMXCondition(o);
    }
}
