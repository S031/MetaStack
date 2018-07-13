#if NETCOREAPP
using S031.MetaStack.Core.Data;
#else
using S031.MetaStack.WinForms.Data;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETCOREAPP
namespace S031.MetaStack.Core
#else
namespace S031.MetaStack.WinForms
#endif
{
	public class AttribInfo
	{
		public AttribInfo()
		{
			Locate = true;
			DataType = "String";
			Width = 30;
			DisplayWidth = 10;
			AttribName = string.Empty;
			AttribPath = string.Empty;
			Name = string.Empty;
			Position = 0;
			Mask = string.Empty;
			Format = string.Empty;
			IsPK = false;
			Visible = true;
			ReadOnly = false;
			Enabled = true;
			Sorted = false;
			SuperMethod = string.Empty;
			SuperFilter = string.Empty;
			SuperObject = string.Empty;
			ListItems = string.Empty;
			ListItems = string.Empty;
			FieldName = string.Empty;
			ConstName = string.Empty;
			Agregate = string.Empty;
		}
		public string AttribName { get; set; }
		public string AttribPath { get; set; }
		public string Name { get; set; }
		public int Position { get; set; }
		public string DataType { get; set; }
		public int Width { get; set; }
		public int DisplayWidth { get; set; }
		public string Mask { get; set; }
		public string Format { get; set; }
		public bool IsPK { get; set; }
		public bool Locate { get; set; }
		public bool Visible { get; set; }
		public bool ReadOnly { get; set; }
		public bool Enabled { get; set; }
		public bool Sorted { get; set; }
		public string SuperForm { get; set; }
		public string SuperObject { get; set; }
		public string SuperMethod { get; set; }
		public string SuperFilter { get; set; }
		public string ListItems { get; set; }
		public string ListData { get; set; }
		public string FieldName { get; set; }
		public string ConstName { get; set; }
		public string Agregate { get; set; }
	}

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
	}

	public class InterfaceInfo
	{
		public InterfaceInfo()
		{
			InterfaceParameters = new ParamInfoList();
		}
		public string InterfaceID { get; set; }
		public string InterfaceName { get; set; }
		public string Description { get; set; }
		public bool MultipleRowsParams { get; set; }
		public bool MultipleRowsResult { get; set; }
		public ParamInfoList InterfaceParameters { get; private set; }
	}

	public enum TransactionActionSupport
	{
		None = 0,
		Support = 1,
		Required = 2
	}
	public enum ActionWebAuthenticationType
	{
		None = 0,
		Basic = 1,
		Certificate = 2,
		NotAllowed = -1
	}

	public class ActionInfo : InterfaceInfo
	{
		public string ActionID { get; set; }
		public string AssemblyID { get; set; }
		public string ClassName { get; set; }
		public string Name { get; set; }
		public bool LogOnError { get; set; }
		public bool EMailOnError { get; set; }
		public string EMailGroup { get; set; }
		public TransactionActionSupport TransactionSupport { get; set; }
		public DataPackage GetInputParamTable()
		{
			return new DataPackage("t_" + this.ActionID,
				this.InterfaceParameters.InputParameters().Select(pi => pi.ParameterID).ToArray());
		}
		public DataPackage GetInputParamTable(params object[] paramList)
		{
			return new DataPackage("t_" + this.ActionID,
				this.InterfaceParameters.InputParameters().Select(pi => pi.ParameterID).ToArray(), paramList);
		}
		public DataPackage GetOutputParamTable()
		{
			return new DataPackage("t_" + this.ActionID,
				this.InterfaceParameters.OutputParameters().Select(pi => pi.ParameterID).ToArray());
		}
		public ActionWebAuthenticationType WebAuthentication { get; set; }
	}
}
