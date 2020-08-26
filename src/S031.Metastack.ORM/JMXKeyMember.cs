using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public sealed class JMXKeyMember : JsonSerializible
	{
		public JMXKeyMember() : base(null) { }

		public string FieldName { get; set; }
		public int Position { get; set; }
		public bool IsDescending { get; set; }
		public bool IsIncluded { get; set; }
		public static bool operator ==(JMXKeyMember m1, JMXKeyMember m2)
		{
			return (m1.FieldName == m2.FieldName &&
				m1.Position == m2.Position &&
				m1.IsDescending == m2.IsDescending &&
				m1.IsIncluded == m2.IsIncluded);
		}
		public static bool operator !=(JMXKeyMember m1, JMXKeyMember m2)
		{
			return (m1.FieldName != m2.FieldName ||
				m1.Position != m2.Position ||
				m1.IsDescending != m2.IsDescending ||
				m1.IsIncluded != m2.IsIncluded);
		}
		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}
		public override int GetHashCode()
		{
			return new { FieldName, Position, IsDescending, IsIncluded }.GetHashCode();
		}
		public bool IsEmpty() => string.IsNullOrEmpty(FieldName);

		internal JMXKeyMember(JsonObject o)
			: base(o)
		{
		}
		internal static JMXKeyMember ReadFrom(JsonObject o)
			=> new JMXKeyMember(o);
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("FieldName", FieldName);
			writer.WriteProperty("Position", Position);
			writer.WriteProperty("IsDescending", IsDescending);
			writer.WriteProperty("IsIncluded", IsIncluded);
		}
		public override void FromJson(JsonValue source)
		{
			var o = source as JsonObject;
			FieldName = o.GetStringOrDefault("FieldName");
			Position = o.GetIntOrDefault("Position");
			IsDescending = o.GetBoolOrDefault("IsDescending");
			IsIncluded = o.GetBoolOrDefault("IsIncluded");
		}
	}
}
