using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public readonly struct JMXIdentity
	{
		public JMXIdentity(bool isIdentity=false, int seed = 0, int increment=0)
		{
			IsIdentity = isIdentity;
			Seed = isIdentity && seed <= 0 ? 1 : seed;
			Increment = isIdentity && increment == 0 ? 1 : increment;
		}
        public bool IsIdentity { get; }
        public int Seed { get; }
        public int Increment { get; }
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
			writer.WriteProperty("IsIdentity", IsIdentity);
			writer.WriteProperty("Seed", Seed);
			writer.WriteProperty("Increment", Increment);
		}
		internal JMXIdentity(JsonObject o)
		{
			IsIdentity = o.GetBoolOrDefault("IsIdentity");
			Seed = o.GetIntOrDefault("Seed", 1);
			Increment = o.GetIntOrDefault("Increment", 1);
		}
		internal static JMXIdentity ReadFrom(JsonObject o)
			=> new JMXIdentity(o);
	}
}
