using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using S031.MetaStack.Common;
using System.Linq;
using S031.MetaStack.Core.Security;

namespace S031.MetaStack.Core.Actions
{
	public class ActionManager : ManagerObjectBase, IDisposable
	{
		private static readonly object obj4Lock = new object();
		private static readonly Dictionary<string, ActionInfo> _actions = ActionsList.CreateActionsList();

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
							Case When SuperObject Like '%SysSchema%' and SuperMethod = 'ObjectName' then 1 else 0 end as IsObjectName
						From {1}InterfaceParameters Where InterfaceID = '{0}'";

		private readonly string _schemaName = string.Empty;

		public ActionManager(MdbContext mdbContext) : base(mdbContext)
		{
			if (mdbContext.ConnectInfo.SchemaSupport)
				_schemaName = "SysCat.";
		}

		public DataPackage Execute(string actionID, DataPackage inParamStor)
		{
			ActionInfo ai = GetActionInfo(actionID);
			try
			{
				return ExecuteInternal(ai, inParamStor);
			}
			catch (Exception ex)
			{
				if (ai == null || ai.LogOnError)
					Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
				//Костыль!!!
				//if (ai != null && ai.EMailOnError)
				//	Comm.SendEMail(ai.EMailGroup, "Ошибка выполнения операции '{0}'".ToFormat(ai.ActionID), ex.Detail.FullReport);
				throw;
			}
		}
		public async Task<DataPackage> ExecuteAsync(string actionID, DataPackage inParamStor)
		{
			ActionInfo ai = GetActionInfo(actionID);
			try
			{
				return await ExecuteInternalAsync(ai, inParamStor);
			}
			catch (Exception ex)
			{
				if (ai == null || ai.LogOnError)
					Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
				//Костыль!!!
				//if (ai != null && ai.EMailOnError)
				//	Comm.SendEMail(ai.EMailGroup, "Ошибка выполнения операции '{0}'".ToFormat(ai.ActionID), ex.Detail.FullReport);
				throw;
			}
		}

		private static DataPackage ExecuteInternal(ActionInfo ai, DataPackage inParamStor)
		{
			var se = CreateEvaluator(ai, inParamStor);
			return se.Invoke(ai, inParamStor);
		}

		private static async Task<DataPackage> ExecuteInternalAsync(ActionInfo ai, DataPackage inParamStor)
		{
			var se = CreateEvaluator(ai, inParamStor);
			//!!! AuthorizationRequired remove from sys action list and add test authentication in procedure
			if (ai.AuthorizationRequired)
			{
				string objectNameAttribute = ai.InterfaceParameters.GetObjectNameParamInfo()?.AttribName ?? ai.ActionID;
				inParamStor.GoDataTop();
				if (!ApplicationContext
					.GetAuthorizationProvider()
					.HasPermission(ai, objectNameAttribute))
					throw new AuthorizationExceptions(ai.GetAuthorizationExceptionsMessage(objectNameAttribute));
				inParamStor.GoDataTop();
			}
			return await se.InvokeAsync(ai, inParamStor);
		}

		private static IAppEvaluator CreateEvaluator(ActionInfo ai, DataPackage inParamStor)
		{
			var inParams = ai.InterfaceParameters.Where(kvp => kvp.Value.Dirrect == ParamDirrect.Input && kvp.Value.Required).Select(kvp => kvp.Value);
			int pCount = inParams.Count();
			if (inParamStor != null) inParamStor.GoDataTop();
			if (pCount > 0 && (inParamStor == null || inParamStor.FieldCount < pCount || !inParamStor.Read()))
			{
				throw new InvalidOperationException(
					"Количество параметров операции '{0}', не соответствует интерфейсу '{1}'"
					.ToFormat(ai.ActionID, ai.InterfaceID));
			}
			else if (pCount > 0)
			{
				foreach (ParamInfo pi in inParams)
				{
					try
					{
						var val = inParamStor[pi.ParameterID];
					}
					catch
					{
						throw new InvalidOperationException(
							"Обязательный параметер интерфейса '{0}.{1}', не найден в списке параметров  операции '{2}'"
							.ToFormat(ai.InterfaceID, pi.ParameterID, ai.ActionID));
					}
				}
				inParamStor.GoDataTop();
			}
			IAppEvaluator se = ImplementsList.GetTypes(typeof(IAppEvaluator))
				.FirstOrDefault(t => t.FullName.Equals(ai.ClassName, StringComparison.CurrentCultureIgnoreCase))?
				.CreateInstance<IAppEvaluator>();
			return se;
		}

		public async Task<ActionInfo> GetActionInfoAsync(string actionID)
		{
			actionID.NullTest(nameof(actionID));
			if (!_actions.ContainsKey(actionID))
			{
				ActionInfo ai = new ActionInfo();
				MdbContext catContext = GetMdbContext(ContextTypes.SysCat);
				using (var dr = await catContext.GetReaderAsync(_sql_actions.ToFormat(actionID, _schemaName)))
				{
					if (dr.Read())
						SetActionAttributes(ai, dr);
					else
						ai = null;
				}
				if (ai != null)
				{
					using (var dr = await GetMdbContext().GetReaderAsync(_sql_parameters.ToFormat(ai.IID, _schemaName)))
					{
						ParamInfoList pil = ai.InterfaceParameters;
						for (; dr.Read();)
							SetParamAttributes(pil, dr);
					}
				}
				else
					throw new KeyNotFoundException($"Action {actionID} not found");

				if (!_actions.ContainsKey(actionID))
					lock (obj4Lock) { _actions.Add(actionID, ai); }
			}
			return _actions[actionID];
		}
		public ActionInfo GetActionInfo(string actionID)
		{
			actionID.NullTest(nameof(actionID));
			if (!_actions.ContainsKey(actionID))
			{
				ActionInfo ai = new ActionInfo();
				MdbContext catContext = GetMdbContext(ContextTypes.SysCat);
				using (var dr = catContext.GetReader(_sql_actions.ToFormat(actionID, _schemaName)))
				{
					if (dr.Read())
						SetActionAttributes(ai, dr);
					else
						ai = null;
				}
				if (ai != null)
				{
					using (var dr = GetMdbContext().GetReader(_sql_parameters.ToFormat(ai.IID, _schemaName)))
					{
						ParamInfoList pil = ai.InterfaceParameters;
						for (; dr.Read();)
							SetParamAttributes(pil, dr);
					}
				}
				else
					throw new KeyNotFoundException($"Action {actionID} not found");

				if (!_actions.ContainsKey(actionID))
					lock (obj4Lock) { _actions.Add(actionID, ai); }
			}
			return _actions[actionID];
		}

		private void SetActionAttributes(ActionInfo ai, DataPackage dr)
		{
			ai.ActionID = (string)dr["ActionID"];
			ai.AssemblyID = (string)dr["AssemblyID"];
			ai.ClassName = (string)dr["ClassName"];
			ai.Name = (string)dr["Name"];
			ai.LogOnError = (bool)dr["LogOnError"];
			ai.EMailOnError = (bool)dr["EMailOnError"];
			ai.EMailGroup = (string)dr["EMailGroup"];
			ai.IID = dr["IID"].CastOf<int>();
			ai.InterfaceID = (string)dr["InterfaceID"];
			ai.InterfaceName = (string)dr["Name"];
			ai.Description = (string)dr["Description"];
			ai.MultipleRowsParams = (bool)dr["MultipleRowsParams"];
			ai.MultipleRowsResult = (bool)dr["MultipleRowsResult"];
			ai.TransactionSupport = (TransactionActionSupport)dr["TransactionSupport"];
			ai.AuthenticationRequired = (bool)dr["AuthenticationRequired"];
			ai.AuthorizationRequired = (bool)dr["AuthorizationRequired"];
			ai.WebAuthentication = ai.AuthenticationRequired ? ActionWebAuthenticationType.Basic : ActionWebAuthenticationType.None;
			ai.AsyncMode = (bool)dr["AsyncMode"];
		}
		private void SetParamAttributes(ParamInfoList pil, DataPackage dr)
		{
			pil.Add(new ParamInfo()
			{
				ParameterID = (string)dr["ParameterID"],
				Dirrect = (ParamDirrect)dr["Dirrect"],
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
				IsObjectName = (bool)dr["IsObjectName"]
			});
		}
	}
}
