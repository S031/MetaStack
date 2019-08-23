using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public readonly struct JMXDataSize
	{
		public JMXDataSize(int size = 0, int scale = 0, int precision = 0)
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
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			writer.WriteStartObject();
			ToStringRaw(writer);
			writer.WriteEndObject();
			return writer.ToString();
		}
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("Size", Size);
			writer.WriteProperty("Scale", Scale);
			writer.WriteProperty("Precision", Precision);
		}
		internal JMXDataSize(JsonObject o)
		{
			Size = o.GetIntOrDefault("Size");
			Scale = o.GetIntOrDefault("Scale");
			Precision = o.GetIntOrDefault("Precision");
		}
		internal static JMXDataSize ReadFrom(JsonObject o)
			=> new JMXDataSize(o);
	}
}
