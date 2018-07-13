using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
			IsOutput = false;
		}
		public JMXParameter(string paramName):base(paramName)
		{
			PresentationType = string.Empty;
			IsOutput = false;
		}
		public string ParamName { get => AttribName; set => AttribName = value; }
		public bool IsOutput { get; set; }
		public string PresentationType { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				writer.WriteProperty("IsOutput", IsOutput);
				writer.WriteProperty("PresentationType", PresentationType);
				ToStringRaw(writer);
				writer.WriteEndObject();
				return sb.ToString();
			}
		}

	}
}
