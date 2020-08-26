using S031.MetaStack.Data;
using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public sealed class JMXParameter : JMXAttribute
	{
		public JMXParameter():base()
		{
			PresentationType = string.Empty;
			Dirrect = ParamDirrect.Input;
		}

		public JMXParameter(string paramName):base(paramName)
		{
			PresentationType = string.Empty;
			Dirrect = ParamDirrect.Input;
		}

		public string ParamName { get => AttribName; set => AttribName = value; }

		public ParamDirrect Dirrect { get; set; }

		public bool NullIfEmpty { get; set; }

		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			writer.WriteProperty("NullIfEmpty", NullIfEmpty);
			base.ToJsonRaw(writer);
		}
		internal JMXParameter(JsonObject o) : base(o)
		{
			Dirrect = o.GetEnum<ParamDirrect>("Dirrect");
			NullIfEmpty = o.GetBoolOrDefault("NullIfEmpty");
		}
		internal static new JMXParameter ReadFrom(JsonObject o)
			=> new JMXParameter(o);
	}
}
