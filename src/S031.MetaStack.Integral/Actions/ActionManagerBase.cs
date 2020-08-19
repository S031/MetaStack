using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Actions
{
	public abstract class ActionManagerBase : IActionManager
	{
		private static readonly MapTable<string, ActionInfo> _actionsCache = new MapTable<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);

		const string _sql_actions = @"
			Select 
				QA.ActionName As ActionID,
				QA.AssemblyID,
				QA.ClassName,
				QA.Name,
				QA.LogOnError,
				QA.EMailOnError,
				COALESCE(QA.EMailGroup, '') As EMailGroup,
				QA.AuthenticationRequired,
				QA.AuthorizationRequired,
				QA.AsyncMode,
				QA.TransactionSupport,
				QA.IsStatic,
				1 As WebAuthentication,
				I.ID As IID,
				I.InterfaceName As InterfaceID,
				I.Name,
				COALESCE(I.Description, '') As Description,
				I.MultipleRowsParams,
				I.MultipleRowsResult
			From {1}Actions QA Inner Join {1}Interfaces I On QA.InterfaceID = I.ID
			Where QA.ActionName = '{0}'";

		const string _sql_parameters = @"
			Select ParameterName As ParameterID,
				Dirrect,
				Position,
				Name,
				DataType,
				Width,
				Coalesce(DisplayWidth, 0) As DisplayWidth,
				Coalesce(PresentationType, '') As PresentationType,
				Coalesce(Mask, '') As Mask,
				Coalesce(Format, '') As Format,
				Coalesce(SuperObject, '') As SuperObject,
				Coalesce(SuperMethod, '') As SuperMethod,
				Coalesce(SuperForm, '') As SuperForm,
				Coalesce(SuperFilter, '') As SuperFilter,
				Coalesce(ListItems, '') As ListItems,
				Coalesce(ListData, '') As ListData,
				ReadOnly,
				Required,
				Visible,
				Coalesce(DefaultValue, '') As DefaultValue,
				Coalesce(ConstantName, '') As ConstantName,
				Coalesce(FieldName, '') As FieldName,
				Cast(Case When SuperObject Like '%SysSchema%' and SuperMethod = 'ObjectName' then 1 else 0 end as bit) as IsObjectName
			From {1}InterfaceParameters Where InterfaceID = '{0}'";

		private readonly string _schemaName = string.Empty;

		protected MdbContext SysCatDbContext;

		public abstract DataPackage Execute(string actionID, DataPackage inParamStor);

		public abstract Task<DataPackage> ExecuteAsync(string actionID, DataPackage inParamStor);

		public ActionInfo GetActionInfo(string actionID)
		{
			actionID.NullTest(nameof(actionID));
			if (!_actionsCache.TryGetValue(actionID, out ActionInfo ai))
			{
				using (var dr = SysCatDbContext.GetReader(_sql_actions.ToFormat(actionID, _schemaName)))
				{
					if (dr.Read())
						ai = SetActionAttributes(dr);
				}
				if (ai != null)
				{
					using (var dr = SysCatDbContext.GetReader(_sql_parameters.ToFormat(ai.IID, _schemaName)))
					{
						ParamInfoList pil = ai.InterfaceParameters;
						for (; dr.Read();)
							pil.Add(SetParamAttributes(dr));
					}
				}
				else
					throw new KeyNotFoundException($"Action {actionID} not found");

				_actionsCache.TryAdd(actionID, ai);
			}
			return ai;
		}

		public async Task<ActionInfo> GetActionInfoAsync(string actionID)
		{
			actionID.NullTest(nameof(actionID));
			if (!_actionsCache.TryGetValue(actionID, out ActionInfo ai))
			{
				using (var dr = await SysCatDbContext.GetReaderAsync(_sql_actions.ToFormat(actionID, _schemaName)))
				{
					if (dr.Read())
						ai = SetActionAttributes(dr);
				}
				if (ai != null)
				{
					using (var dr = await SysCatDbContext.GetReaderAsync(_sql_parameters.ToFormat(ai.IID, _schemaName)))
					{
						ParamInfoList pil = ai.InterfaceParameters;
						for (; dr.Read();)
							pil.Add(SetParamAttributes(dr));
					}
				}
				else
					throw new KeyNotFoundException($"Action {actionID} not found");

				_actionsCache.TryAdd(actionID, ai);
			}
			return ai;
		}

		private static ActionInfo SetActionAttributes(DataPackage dr)
			=> new ActionInfo
			{
				ActionID = (string)dr["ActionID"],
				AssemblyID = (string)dr["AssemblyID"],
				ClassName = (string)dr["ClassName"],
				Name = (string)dr["Name"],
				LogOnError = (bool)dr["LogOnError"],
				EMailOnError = (bool)dr["EMailOnError"],
				EMailGroup = (string)dr["EMailGroup"],
				IID = dr["IID"].CastOf<int>(),
				InterfaceID = (string)dr["InterfaceID"],
				InterfaceName = (string)dr["Name"],
				Description = (string)dr["Description"],
				MultipleRowsParams = (bool)dr["MultipleRowsParams"],
				MultipleRowsResult = (bool)dr["MultipleRowsResult"],
				TransactionSupport = (TransactionActionSupport)dr["TransactionSupport"],
				AuthenticationRequired = (bool)dr["AuthenticationRequired"],
				AuthorizationRequired = (bool)dr["AuthorizationRequired"],
				WebAuthentication = (bool)dr["AuthenticationRequired"] ? ActionWebAuthenticationType.Basic : ActionWebAuthenticationType.None,
				AsyncMode = (bool)dr["AsyncMode"],
				IsStatic = (bool)dr["IsStatic"]
			};

		private static ParamInfo SetParamAttributes(DataPackage dr)
			=> new ParamInfo()
			{
				ParameterID = (string)dr["ParameterID"],
				Dirrect = (ParamDirrect)(int)dr["Dirrect"],
				Position = dr["Position"].CastOf<int>(),
				Name = (string)dr["Name"],
				DataType = (string)dr["DataType"],
				Width = dr["Width"].CastOf<int>(),
				DisplayWidth = dr["DisplayWidth"].CastOf<int>(),
				PresentationType = (string)dr["PresentationType"],
				Mask = (string)dr["Mask"],
				Format = (string)dr["Format"],
				SuperObject = (string)dr["SuperObject"],
				SuperMethod = (string)dr["SuperMethod"],
				SuperForm = (string)dr["SuperForm"],
				SuperFilter = (string)dr["SuperFilter"],
				ListItems = (string)dr["ListItems"],
				ListData = (string)dr["ListData"],
				ReadOnly = (bool)dr["ReadOnly"],
				Required = (bool)dr["Required"],
				Visible = (bool)dr["Visible"],
				DefaultValue = (string)dr["DefaultValue"],
				ConstName = (string)dr["ConstantName"],
				FieldName = (string)dr["FieldName"],
				IsObjectName = dr["IsObjectName"].CastOf<bool>()
			};

		public static async Task AddToCache(IEnumerable<KeyValuePair<string, ActionInfo>> listOfActions)
			=> await Task.Run(() =>
			   {
				   _actionsCache.Clear();
				   foreach (var ai in listOfActions)
					   _actionsCache.Add(ai.Key, ai.Value);
			   });
	}
}
