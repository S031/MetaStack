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
			return this.Where(kvp => kvp.Key > (int)ParamDirrect.Output).Select(kvp => kvp.Value);
		}
		public ParamInfo GetObjectNameParamInfo(ParamDirrect dirrect = ParamDirrect.Input)
			=>
			dirrect == ParamDirrect.Input
			? InputParameters().FirstOrDefault(p => p.IsObjectName)
			: OutputParameters().FirstOrDefault(p => p.IsObjectName);
	}

}
