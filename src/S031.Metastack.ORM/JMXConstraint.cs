using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public enum JMXConstraintTypes
	{
		none,
		checkConstraint,
		defaultConstraint
	}
    public sealed class JMXConstraint : JsonSerializible
    {
        public JMXConstraint(JMXConstraintTypes constraintType) : this(constraintType, string.Empty, string.Empty)
        {
        }

        public JMXConstraint(JMXConstraintTypes constraintType, string definition) : this(constraintType, "", definition)
        {
        }

        public JMXConstraint(JMXConstraintTypes constraintType, string constraintName, string definition)
            : base(null)
        {
            ConstraintType = constraintType;
            ConstraintName = constraintName;
            Definition = definition;
            CheckOption = true;
        }

        public string ConstraintName { get; set; } = string.Empty;

        public string Definition { get; set; } = string.Empty;

        public JMXConstraintTypes ConstraintType { get; set; } = JMXConstraintTypes.none;

        public bool CheckOption { get; set; } = true;

        public static bool operator ==(JMXConstraint m1, JMXConstraint m2)
        {
            return (m1.ConstraintName == m2.ConstraintName &&
                m1.Definition == m2.Definition &&
                m1.ConstraintType == m2.ConstraintType);
        }

        public static bool operator !=(JMXConstraint m1, JMXConstraint m2)
        {
            return (m1.ConstraintName != m2.ConstraintName ||
                m1.Definition != m2.Definition ||
                m1.ConstraintType != m2.ConstraintType);
        }

        public override bool Equals(object obj)
            => this.GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
            => new { ConstraintName, Definition, ConstraintType }.GetHashCode();

        public bool IsEmpty() => string.IsNullOrEmpty(Definition);

        public override string ToString()
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }

        internal JMXConstraint(JsonValue value): base(value)
        {
        }

        internal static JMXConstraint ReadFrom(JsonObject o)
            => new JMXConstraint(o);

        protected override void ToJsonRaw(JsonWriter writer)
        {
            writer.WriteProperty("ConstraintType", ConstraintType.ToString());
            writer.WriteProperty("Definition", Definition);
            writer.WriteProperty("CheckOption", CheckOption);
        }

		public override void FromJson(JsonValue source)
		{
            JsonObject o = source as JsonObject;
            ConstraintType = o.GetEnum<JMXConstraintTypes>("ConstraintType", JMXConstraintTypes.none);
            Definition = o.GetStringOrDefault("Definition");
            CheckOption = o.GetBoolOrDefault("CheckOption", true);
            ConstraintName = o.GetStringOrDefault("ConstraintName");
		}
	}
}
