﻿#if NETCOREAPP
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
#else
using S031.MetaStack.WinForms.Data;
#endif
using System.Linq;

#if NETCOREAPP
namespace S031.MetaStack.Core.Actions
#else
namespace S031.MetaStack.WinForms.Actions
#endif
{
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
		public bool AuthenticationRequired { get; set; }
		public bool AuthorizationRequired { get; set; }
		public bool AsyncMode { get; set; } = false;
	}
}
