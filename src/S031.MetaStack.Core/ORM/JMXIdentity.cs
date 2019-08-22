using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public struct JMXIdentity
	{
		public JMXIdentity(bool isIdentity=false, int seed = 0, int increment=0)
		{
			IsIdentity = isIdentity;
			Seed = isIdentity && seed <= 0 ? 1 : seed;
			Increment = isIdentity && increment == 0 ? 1 : increment;
		}
        public bool IsIdentity { get; set; }
        public int Seed { get; set; }
        public int Increment { get; set; }
		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("IsIdentity", IsIdentity);
			writer.WriteProperty("Seed", Seed);
			writer.WriteProperty("Increment", Increment);
			writer.WriteEndObject();
		}
		internal static JMXIdentity ReadFrom(JsonObject o)
		{
			var identity = new JMXIdentity()
			{
				IsIdentity = o.GetBoolOrDefault("IsIdentity"),
				Seed = o.GetIntOrDefault("Seed"),
				Increment = o.GetIntOrDefault("Increment")
			};
			return identity;
		}

	}
}
