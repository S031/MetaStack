using S031.MetaStack.Json;
using System.Collections.Generic;
using System.Linq;

#if NETCOREAPP
namespace S031.MetaStack.Core.Actions
#else
namespace S031.MetaStack.WinForms.Actions
#endif
{
	public enum ParamDirrect
	{
		Input = 1000,
		Output = 2000
	}

	public class ParamInfo : AttribInfo
	{
		public ParamInfo()
			: base()
		{
			PresentationType = string.Empty;
			Required = false;
			DefaultValue = string.Empty;
			Dirrect = ParamDirrect.Input;
		}
		public string ParameterID
		{
			get { return this.AttribName; }
			set { this.AttribName = value; }
		}
		public ParamDirrect Dirrect { get; set; }
		public string PresentationType { get; set; }
		public bool Required { get; set; }
		public string DefaultValue { get; set; }
		public bool IsObjectName { get; set; }
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("ParameterID", ParameterID);
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			writer.WriteProperty("PresentationType", PresentationType);
			writer.WriteProperty("Required", Required);
			writer.WriteProperty("DefaultValue", DefaultValue);
			writer.WriteProperty("IsObjectName", IsObjectName);
			writer.WriteProperty("AttribName", AttribName);
			writer.WriteProperty("AttribPath", AttribPath);
			writer.WriteProperty("Name", Name);
			writer.WriteProperty("Position", Position);
			writer.WriteProperty("DataType", DataType);
			writer.WriteProperty("Width", Width);
			writer.WriteProperty("DisplayWidth", DisplayWidth);
			writer.WriteProperty("Mask", Mask);
			writer.WriteProperty("Format", Format);
			writer.WriteProperty("IsPK", IsPK);
			writer.WriteProperty("Locate", Locate);
			writer.WriteProperty("Visible", Visible);
			writer.WriteProperty("ReadOnly", ReadOnly);
			writer.WriteProperty("Enabled", Enabled);
			writer.WriteProperty("Sorted", Sorted);
			writer.WriteProperty("SuperForm", SuperForm);
			writer.WriteProperty("SuperObject", SuperObject);
			writer.WriteProperty("SuperMethod", SuperMethod);
			writer.WriteProperty("SuperFilter", SuperFilter);
			writer.WriteProperty("ListItems", ListItems);
			writer.WriteProperty("ListData", ListData);
			writer.WriteProperty("FieldName", FieldName);
			writer.WriteProperty("ConstName", ConstName);
			writer.WriteProperty("Agregate", Agregate);
		}
		public override string ToString()
		{
			JsonWriter w = new JsonWriter(Formatting.None);
			w.WriteStartObject();
			ToStringRaw(w);
			w.WriteEndObject();
			return w.ToString();
		}
	}

	public class ParamInfoList : SortedList<int, ParamInfo>
	{
		public void Add(ParamInfo pi)
		{
			base.Add((int)pi.Dirrect + pi.Position, pi);
		}
		public IEnumerable<ParamInfo> InputParameters()
		{
			return this.Where(kvp => kvp.Key < (int)ParamDirrect.Output).Select(kvp => kvp.Value);
		}
		public IEnumerable<ParamInfo> OutputParameters()
		{
			return this.Where(kvp => kvp.Key >= (int)ParamDirrect.Output).Select(kvp => kvp.Value);
		}
		public ParamInfo GetObjectNameParamInfo(ParamDirrect dirrect = ParamDirrect.Input)
			=>
			dirrect == ParamDirrect.Input
			? InputParameters().FirstOrDefault(p => p.IsObjectName)
			: OutputParameters().FirstOrDefault(p => p.IsObjectName);
	}

}
