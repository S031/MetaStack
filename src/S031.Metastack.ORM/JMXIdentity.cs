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
        public bool IsIdentity { get; }
        public int Seed { get; }
        public int Increment { get; }
        public override string ToString()
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }
        internal JMXIdentity(JsonValue value)
            : base(value)
        {
            JsonObject o = value as JsonObject;
            IsIdentity = o.GetBoolOrDefault("IsIdentity");
            Seed = o.GetIntOrDefault("Seed", 1);
            Increment = o.GetIntOrDefault("Increment", 1);
        }
        internal static JMXIdentity ReadFrom(JsonObject o)
            => new JMXIdentity(o);

        public override void ToJson(JsonWriter writer)
        {
            writer.WriteStartObject();
            ToJsonRaw(writer);
            writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
        {
            writer.WriteProperty("IsIdentity", IsIdentity);
            writer.WriteProperty("Seed", Seed);
            writer.WriteProperty("Increment", Increment);
        }
    }
}
