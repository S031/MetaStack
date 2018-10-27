using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Converters;
#if NETCOREAPP
using S031.MetaStack.Core.Data;
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

		[JsonConverter(typeof(StringEnumConverter))]
		public Actions.ParamDirrect Dirrect { get; set; }

		public override void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			base.ToStringRaw(writer);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				ToStringRaw(writer);
				writer.WriteEndObject();
				return sb.ToString();
			}
		}

	}
}
