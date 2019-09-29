using S031.MetaStack.Json;
#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Data;
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public sealed class JMXParameter : JMXAttribute
	{
		public JMXParameter():base()
		{
			PresentationType = string.Empty;
			Dirrect = Actions.ParamDirrect.Input;
		}

		public JMXParameter(string paramName):base(paramName)
		{
			PresentationType = string.Empty;
			Dirrect = Actions.ParamDirrect.Input;
		}

		public string ParamName { get => AttribName; set => AttribName = value; }

		public Actions.ParamDirrect Dirrect { get; set; }

		public bool NullIfEmpty { get; set; }

		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			writer.WriteProperty("NullIfEmpty", NullIfEmpty);
			base.ToJsonRaw(writer);
		}

        public override void ToJson(JsonWriter writer)
        {
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
		}

		internal JMXParameter(JsonObject o) : base(o)
		{
			Dirrect = o.GetEnum<Actions.ParamDirrect>("Dirrect");
			NullIfEmpty = o.GetBoolOrDefault("NullIfEmpty");
		}
		internal static new JMXParameter ReadFrom(JsonObject o)
			=> new JMXParameter(o);
	}
}
