using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
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
        public int Size { get; }
        public int Scale { get; }
        public int Precision { get; }
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

        public override string ToString()
           => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }

        internal JMXDataSize(JsonValue value)
            : base(value)
        {
            JsonObject o = value as JsonObject;
            Size = o.GetIntOrDefault("Size");
            Scale = o.GetIntOrDefault("Scale");
            Precision = o.GetIntOrDefault("Precision");
        }

		internal static JMXDataSize ReadFrom(JsonObject o)
			=> new JMXDataSize(o);

        public override void ToJson(JsonWriter writer)
        {
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
        {
			writer.WriteProperty("Size", Size);
			writer.WriteProperty("Scale", Scale);
			writer.WriteProperty("Precision", Precision);
        }
    }
}
