using System.Linq;
using S031.MetaStack.Json;
using System;
using S031.MetaStack.Data;

namespace S031.MetaStack.Actions
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
		Token = 3,
		NotAllowed = -1
	}

	public sealed class ActionInfo : InterfaceInfo
	{
		public const string ObjectNameForStaticActions = "__default";
		public ActionInfo() : base(null) { }
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
		protected override void ToJsonRaw(JsonWriter writer)
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
				item.Value.ToJson(writer);
			writer.WriteEndArray();
		}
		public override void FromJson(JsonValue source)
		{
			JsonObject j = (source as JsonObject);
			ActionID = j.GetStringOrDefault("ActionID");
			AssemblyID = j.GetStringOrDefault("AssemblyID");
			ClassName = j.GetStringOrDefault("ClassName");
			Name = j.GetStringOrDefault("Name");
			LogOnError = j.GetBoolOrDefault("LogOnError");
			EMailOnError = j.GetBoolOrDefault("EMailOnError");
			EMailGroup = j.GetStringOrDefault("EMailGroup");
#if NETCOREAPP
			TransactionSupport = Enum.Parse<TransactionActionSupport>(j.GetStringOrDefault("TransactionSupport"));
			WebAuthentication = Enum.Parse<ActionWebAuthenticationType>(j.GetStringOrDefault("WebAuthentication"));
#else
			TransactionSupport = (TransactionActionSupport)Enum.Parse(typeof(TransactionActionSupport), j.GetStringOrDefault("TransactionSupport"));
			WebAuthentication =(ActionWebAuthenticationType) Enum.Parse(typeof(ActionWebAuthenticationType), j.GetStringOrDefault("WebAuthentication"));
#endif
			AuthenticationRequired = j.GetBoolOrDefault("AuthenticationRequired");
			AuthorizationRequired = j.GetBoolOrDefault("AuthorizationRequired");
			AsyncMode = j.GetBoolOrDefault("AsyncMode");
			IID = j.GetIntOrDefault("IID");
			InterfaceID = j.GetStringOrDefault("InterfaceID");
			InterfaceName = j.GetStringOrDefault("InterfaceName");
			Description = j.GetStringOrDefault("Description");
			MultipleRowsParams = j.GetBoolOrDefault("MultipleRowsParams");
			MultipleRowsResult = j.GetBoolOrDefault("MultipleRowsResult");
			IsStatic = j.GetBoolOrDefault("IsStatic");
			
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
					InterfaceParameters.Add(p);
				}
			}
		}
		internal ActionInfo(JsonObject j) : base(j) { }
		public static ActionInfo Parse(string serializedJsonString)
			=> new ActionInfo((JsonObject)new JsonReader(serializedJsonString).Read());
		public ActionInfo Clone()
		{
			ActionInfo a = new ActionInfo
			{
				ActionID = this.ActionID,
				AssemblyID = this.AssemblyID,
				ClassName = this.ClassName,
				Name = this.Name,
				LogOnError = this.LogOnError,
				EMailOnError = this.EMailOnError,
				EMailGroup = this.EMailGroup,
				TransactionSupport = this.TransactionSupport,
				WebAuthentication = this.WebAuthentication,
				AuthenticationRequired = this.AuthenticationRequired,
				AuthorizationRequired = this.AuthorizationRequired,
				AsyncMode = this.AsyncMode,
				IID = this.IID,
				InterfaceID = this.InterfaceID,
				InterfaceName = this.InterfaceName,
				Description = this.Description,
				MultipleRowsParams = this.MultipleRowsParams,
				MultipleRowsResult = this.MultipleRowsResult,
				IsStatic = this.IsStatic
			};
			foreach (var kvp in InterfaceParameters)
			{
				var v = kvp.Value;
				var p = new ParamInfo()
				{
					ParameterID = v.ParameterID,
					Dirrect = v.Dirrect,
					PresentationType = v.PresentationType,
					Required = v.Required,
					DefaultValue = v.DefaultValue,
					IsObjectName = v.IsObjectName,
					AttribName = v.AttribName,
					AttribPath = v.AttribPath,
					Name = v.Name,
					Position = v.Position,
					DataType = v.DataType,
					Width = v.Width,
					DisplayWidth = v.DisplayWidth,
					Mask = v.Mask,
					Format = v.Format,
					IsPK = v.IsPK,
					Locate = v.Locate,
					Visible = v.Visible,
					ReadOnly = v.ReadOnly,
					Enabled = v.Enabled,
					Sorted = v.Sorted,
					SuperForm = v.SuperForm,
					SuperObject = v.SuperObject,
					SuperMethod = v.SuperMethod,
					SuperFilter = v.SuperFilter,
					ListItems = v.ListItems,
					ListData = v.ListData,
					FieldName = v.FieldName,
					ConstName = v.ConstName,
					Agregate = v.Agregate
				};
				a.InterfaceParameters.Add(kvp.Key, p);
			}
			return a;
		}

//#if NETCOREAPP
		ActionContext _ctx = null;
		public ActionContext GetContext() => _ctx;
		public void SetContext(ActionContext context) => _ctx = context;

		//#endif
	}
}
