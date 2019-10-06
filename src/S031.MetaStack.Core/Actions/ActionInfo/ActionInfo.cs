#if NETCOREAPP
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
#else
using S031.MetaStack.WinForms.Data;
using S031.MetaStack.WinForms.Json;
#endif
using System.Linq;
using S031.MetaStack.Json;
using System;

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

	public sealed class ActionInfo : InterfaceInfo
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
			return new DataPackage(
				this
				.InterfaceParameters
				.InputParameters()
				.Select(pi => $"{pi.ParameterID}.{pi.DataType}.{pi.Width}.{!pi.Required}")
				.ToArray());
		}
		public DataPackage GetInputParamTable(params object[] paramList)
		{
			return new DataPackage(
				this
				.InterfaceParameters
				.InputParameters()
				.Select(pi => $"{pi.ParameterID}.{pi.DataType}.{pi.Width}.{!pi.Required}")
				.ToArray(), paramList);
		}
		public DataPackage GetOutputParamTable()
		{
			return new DataPackage(
				this
				.InterfaceParameters
				.OutputParameters()
				.Select(pi => $"{pi.ParameterID}.{pi.DataType}.{pi.Width}.{!pi.Required}")
				.ToArray());
		}
		public ActionWebAuthenticationType WebAuthentication { get; set; }
		public bool AuthenticationRequired { get; set; }
		public bool AuthorizationRequired { get; set; }
		public bool AsyncMode { get; set; } = false;
		public bool IsStatic { get; set; } = false;
		public int IID { get; set; }

		/// <summary>
		/// Serialize content only <see cref="ActionInfo"/> to json string
		/// </summary>
		/// <returns>json string</returns>
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("ActionID", ActionID);
			writer.WriteProperty("AssemblyID", AssemblyID);
			writer.WriteProperty("ClassName", ClassName);
			writer.WriteProperty("Name", Name);
			writer.WriteProperty("LogOnError", LogOnError);
			writer.WriteProperty("EMailOnError", EMailOnError);
			writer.WriteProperty("EMailGroup", EMailGroup);
			writer.WriteProperty("TransactionSupport", TransactionSupport.ToString());
			writer.WriteProperty("WebAuthentication", WebAuthentication.ToString());
			writer.WriteProperty("AuthenticationRequired", AuthenticationRequired);
			writer.WriteProperty("AuthorizationRequired", AuthorizationRequired);
			writer.WriteProperty("AsyncMode", AsyncMode);
			writer.WriteProperty("IID", IID);
			writer.WriteProperty("InterfaceID", InterfaceID);
			writer.WriteProperty("InterfaceName", InterfaceName);
			writer.WriteProperty("Description", Description);
			writer.WriteProperty("MultipleRowsParams", MultipleRowsParams);
			writer.WriteProperty("MultipleRowsResult", MultipleRowsResult);
			writer.WriteProperty("IsStatic", IsStatic);

			writer.WritePropertyName("InterfaceParameters");
			writer.WriteStartArray();
			foreach (var item in InterfaceParameters)
			{
				writer.WriteStartObject();
				item.Value.ToStringRaw(writer);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		/// <summary>
		/// Serialize <see cref="ActionInfo"/> to json string
		/// </summary>
		/// <returns>json string</returns>
		public override string ToString()
		{
			JsonWriter w = new JsonWriter(Formatting.None);
			w.WriteStartObject();
			ToStringRaw(w);
			w.WriteEndObject();
			return w.ToString();
		}

		public static ActionInfo Create(string serializedJsonString)
		{
			JsonObject j = (JsonObject)(new JsonReader(ref serializedJsonString).Read());
			ActionInfo a = new ActionInfo
			{
				ActionID = j.GetStringOrDefault("ActionID"),
				AssemblyID = j.GetStringOrDefault("AssemblyID"),
				ClassName = j.GetStringOrDefault("ClassName"),
				Name = j.GetStringOrDefault("Name"),
				LogOnError = j.GetBoolOrDefault("LogOnError"),
				EMailOnError = j.GetBoolOrDefault("EMailOnError"),
				EMailGroup = j.GetStringOrDefault("EMailGroup"),
#if NETCOREAPP
				TransactionSupport = Enum.Parse<TransactionActionSupport>(j.GetStringOrDefault("TransactionSupport")),
				WebAuthentication = Enum.Parse<ActionWebAuthenticationType>(j.GetStringOrDefault("WebAuthentication")),
#else
				TransactionSupport = (TransactionActionSupport)Enum.Parse(typeof(TransactionActionSupport), j.GetStringOrDefault("TransactionSupport")),
				WebAuthentication =(ActionWebAuthenticationType) Enum.Parse(typeof(ActionWebAuthenticationType), j.GetStringOrDefault("WebAuthentication")),
#endif
				AuthenticationRequired = j.GetBoolOrDefault("AuthenticationRequired"),
				AuthorizationRequired = j.GetBoolOrDefault("AuthorizationRequired"),
				AsyncMode = j.GetBoolOrDefault("AsyncMode"),
				IID = j.GetIntOrDefault("IID"),
				InterfaceID = j.GetStringOrDefault("InterfaceID"),
				InterfaceName = j.GetStringOrDefault("InterfaceName"),
				Description = j.GetStringOrDefault("Description"),
				MultipleRowsParams = j.GetBoolOrDefault("MultipleRowsParams"),
				MultipleRowsResult = j.GetBoolOrDefault("MultipleRowsResult"),
				IsStatic = j.GetBoolOrDefault("IsStatic")
			};
			if (j.ContainsKey("InterfaceParameters"))
			{
				JsonArray ps = (JsonArray)j["InterfaceParameters"];
				foreach (var v in ps)
				{
					var o = (JsonObject)v;
					var p = new ParamInfo()
					{
						ParameterID = o.GetStringOrDefault("ParameterID"),
#if NETCOREAPP
						Dirrect = Enum.Parse<ParamDirrect>(o.GetStringOrDefault("Dirrect")),
#else
						Dirrect = (ParamDirrect)Enum.Parse(typeof(ParamDirrect), o.GetStringOrDefault("Dirrect")),
#endif
						PresentationType = o.GetStringOrDefault("PresentationType"),
						Required = o.GetBoolOrDefault("Required"),
						DefaultValue = o.GetStringOrDefault("DefaultValue"),
						IsObjectName = o.GetBoolOrDefault("IsObjectName"),
						AttribName = o.GetStringOrDefault("AttribName"),
						AttribPath = o.GetStringOrDefault("AttribPath"),
						Name = o.GetStringOrDefault("Name"),
						Position = o.GetIntOrDefault("Position"),
						DataType = o.GetStringOrDefault("DataType"),
						Width = o.GetIntOrDefault("Width"),
						DisplayWidth = o.GetIntOrDefault("DisplayWidth"),
						Mask = o.GetStringOrDefault("Mask"),
						Format = o.GetStringOrDefault("Format"),
						IsPK = o.GetBoolOrDefault("IsPK"),
						Locate = o.GetBoolOrDefault("Locate"),
						Visible = o.GetBoolOrDefault("Visible"),
						ReadOnly = o.GetBoolOrDefault("ReadOnly"),
						Enabled = o.GetBoolOrDefault("Enabled"),
						Sorted = o.GetBoolOrDefault("Sorted"),
						SuperForm = o.GetStringOrDefault("SuperForm"),
						SuperObject = o.GetStringOrDefault("SuperObject"),
						SuperMethod = o.GetStringOrDefault("SuperMethod"),
						SuperFilter = o.GetStringOrDefault("SuperFilter"),
						ListItems = o.GetStringOrDefault("ListItems"),
						ListData = o.GetStringOrDefault("ListData"),
						FieldName = o.GetStringOrDefault("FieldName"),
						ConstName = o.GetStringOrDefault("ConstName"),
						Agregate = o.GetStringOrDefault("Agregate")
					};
					a.InterfaceParameters.Add(p);
				}
			}
			return a;
		}

#if NETCOREAPP
		ActionContext _ctx = null;
		public ActionContext GetContext() => _ctx;
		public void SetContext(ActionContext context) => _ctx = context;
#endif
	}
}
