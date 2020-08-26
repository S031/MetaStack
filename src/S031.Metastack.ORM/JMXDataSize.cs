using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public sealed class JMXDataSize: JsonSerializible
	{
		public JMXDataSize(int size = 0, int scale = 0, int precision = 0)
            : base(null)
		{
			Size = size;
			Scale = scale;
			Precision = precision;
		}
        public int Size { get; private set; }
        public int Scale { get; private set; }
		public int Precision { get; private set; }
		public static bool operator ==(JMXDataSize s1, JMXDataSize s2)
		{
			return (s1.Size == s2.Size && s1.Scale == s2.Scale && s1.Precision == s2.Precision);
		}
		public static bool operator !=(JMXDataSize s1, JMXDataSize s2)
		{
			return (s1.Size != s2.Size || s1.Scale != s2.Scale || s1.Precision != s2.Precision);
		}
		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}
		public override int GetHashCode()
		{
			return new { Size, Scale, Precision }.GetHashCode();
		}
		public bool IsEmpty() => Size == 0 && Scale == 0 && Precision == 0;

		internal JMXDataSize(JsonValue value)
			: base(value) { }
		internal static JMXDataSize ReadFrom(JsonObject o)
			=> new JMXDataSize(o);
        protected override void ToJsonRaw(JsonWriter writer)
        {
			writer.WriteProperty("Size", Size);
			writer.WriteProperty("Scale", Scale);
			writer.WriteProperty("Precision", Precision);
        }
		public override void FromJson(JsonValue source)
		{
            JsonObject o = source as JsonObject;
            Size = o.GetIntOrDefault("Size");
            Scale = o.GetIntOrDefault("Scale");
            Precision = o.GetIntOrDefault("Precision");
		}
	}
}
