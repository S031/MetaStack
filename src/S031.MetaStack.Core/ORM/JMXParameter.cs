using S031.MetaStack.Json;
#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Data;
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXParameter : JMXAttribute
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

		public override void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			writer.WriteProperty("NullIfEmpty", NullIfEmpty);
			base.ToStringRaw(writer);
		}

		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}

	}
}
