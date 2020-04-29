using S031.MetaStack.Data;
using S031.MetaStack.Json;
using System.Collections.Generic;
using System.Linq;

namespace S031.MetaStack.Actions
{
	//public enum ParamDirrect
	//{
	//	Input = S031.MetaStack.Data.ParamDirrect.Input,
	//	Output = S031.MetaStack.Data.ParamDirrect.Output
	//}

	public class ParamInfo : AttribInfo
	{
		public ParamInfo()
			: base()
		{
		}

		public string ParameterID
		{
			get { return this.AttribName; }
			set { this.AttribName = value; }
		}

		public ParamDirrect Dirrect { get; set; } = ParamDirrect.Input;
		public string PresentationType { get; set; } = string.Empty;
		public bool Required { get; set; } = false;
		public string DefaultValue { get; set; } = string.Empty;
		public bool IsObjectName { get; set; }
		
		internal ParamInfo(JsonObject o):base(o)
		{
			if (o.ContainsKey("Dirrect")) Dirrect = o.GetEnum<ParamDirrect>("Dirrect");
			if (o.ContainsKey("PresentationType")) PresentationType = o["PresentationType"];
			if (o.ContainsKey("Required")) Required = o["Required"];
			if (o.ContainsKey("DefaultValue")) DefaultValue = o["DefaultValue"];
			if (o.ContainsKey("IsObjectName")) IsObjectName = o["IsObjectName"];
		}
		
		protected override void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty("ParameterID", ParameterID);
			writer.WriteProperty("Dirrect", Dirrect.ToString());
			writer.WriteProperty("PresentationType", PresentationType);
			writer.WriteProperty("Required", Required);
			writer.WriteProperty("DefaultValue", DefaultValue);
			writer.WriteProperty("IsObjectName", IsObjectName);
			base.ToJsonRaw(writer);
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
