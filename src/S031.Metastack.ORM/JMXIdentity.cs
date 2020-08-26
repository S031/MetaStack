using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
    public sealed class JMXIdentity : JsonSerializible
    {
        public JMXIdentity(bool isIdentity = false, int seed = 0, int increment = 0)
            : base(null)
        {
            IsIdentity = isIdentity;
            Seed = isIdentity && seed <= 0 ? 1 : seed;
            Increment = isIdentity && increment == 0 ? 1 : increment;
        }
        public bool IsIdentity { get; private set; }
        public int Seed { get; private set; }
        public int Increment { get; private set; }
        
        internal JMXIdentity(JsonValue value)
            : base(value)
        {
        }
        internal static JMXIdentity ReadFrom(JsonObject o)
            => new JMXIdentity(o);
        protected override void ToJsonRaw(JsonWriter writer)
        {
            writer.WriteProperty("IsIdentity", IsIdentity);
            writer.WriteProperty("Seed", Seed);
            writer.WriteProperty("Increment", Increment);
        }
		public override void FromJson(JsonValue source)
		{
            JsonObject o = source as JsonObject;
            IsIdentity = o.GetBoolOrDefault("IsIdentity");
            Seed = o.GetIntOrDefault("Seed", 1);
            Increment = o.GetIntOrDefault("Increment", 1);
		}
	}
}
