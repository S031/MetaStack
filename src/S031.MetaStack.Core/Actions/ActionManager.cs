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

namespace S031.MetaStack.Core.Actions
{
	public class ActionManager : ManagerObjectBase, IDisposable
	{
		private static readonly object obj4Lock = new object();
		private static readonly Dictionary<string, ActionInfo> _actions = ActionsList.CreateActionsList();

		public ActionManager(MdbContext mdbContext) : base(mdbContext)
		{
		}

		public ActionInfo GetActionInfo(string actionID)
		{
			actionID.NullTest(nameof(actionID));
			if (!_actions.ContainsKey(actionID))
				throw new KeyNotFoundException($"Action {actionID} not found");
			return _actions[actionID];
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
				//if (ai != null && ai.EMailOnError)
				//	Comm.SendEMail(ai.EMailGroup, "Ошибка выполнения операции '{0}'".ToFormat(ai.ActionID), ex.Detail.FullReport);
				throw ex;
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
				//if (ai != null && ai.EMailOnError)
				//	Comm.SendEMail(ai.EMailGroup, "Ошибка выполнения операции '{0}'".ToFormat(ai.ActionID), ex.Detail.FullReport);
				throw ex;
			}
		}

		private static DataPackage ExecuteInternal(ActionInfo ai, DataPackage inParamStor)
		{
			var se = CreateEvaluator(ai, inParamStor);
			return se.Invoke(ai, inParamStor);
		}

		private static async Task< DataPackage> ExecuteInternalAsync(ActionInfo ai, DataPackage inParamStor)
		{
			var se = CreateEvaluator(ai, inParamStor);
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
	}
}
