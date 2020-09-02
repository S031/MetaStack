using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskPlus.Server.Api.Properties;
using TaskPlus.Server.Data;

namespace TaskPlus.Server.Actions
{
	public sealed class ActionManager : ActionManagerBase
	{
		private readonly IServiceProvider _services;
		private readonly IConfiguration _config;
		private readonly ILogger _logger;
		private readonly IBasicAuthorizationProvider _authorizationProvider;

		public ActionManager(IServiceProvider services) 
		{
			_services = services;
			_config = services.GetRequiredService<IConfiguration>();
			_logger = services.GetRequiredService<ILogger>();
			_authorizationProvider = services.GetRequiredService<IBasicAuthorizationProvider>();
			this.SysCatDbContext = services
				.GetRequiredService<IMdbContextFactory>()
				.GetContext(Strings.SysCatConnection);
		}

		public override DataPackage Execute(ActionInfo ai, DataPackage inParamStor)
		{
			throw new NotImplementedException();
		}

		public override async Task<DataPackage> ExecuteAsync(ActionInfo ai, DataPackage inParamStor)
		{
			try
			{
				return await ExecuteInternalAsync(ai, inParamStor);
			}
			catch (Exception ex)
			{
				if (ai == null || ai.LogOnError)
					_logger.LogError($"{ex.Message}\n{ex.StackTrace}");
				//Костыль!!!
				//if (ai != null && ai.EMailOnError)
				//	Comm.SendEMail(ai.EMailGroup, "Ошибка выполнения операции '{0}'".ToFormat(ai.ActionID), ex.Detail.FullReport);
				throw;
			}
		}
		
		private async Task<DataPackage> ExecuteInternalAsync(ActionInfo ai, DataPackage inParamStor)
		{
			var se = CreateEvaluator(ai, inParamStor);
			//!!! AuthorizationRequired remove from sys action list and add test authentication in procedure
			if (ai.AuthorizationRequired)
				await AuthorizationAsync(ai, inParamStor);
			return await se.InvokeAsync(ai, inParamStor);
		}

		private async Task AuthorizationAsync(ActionInfo ai, DataPackage inParamStor)
		{
			ParamInfo objectNameParamInfo = ai.InterfaceParameters.GetObjectNameParamInfo();
			string objectName = ActionInfo.ObjectNameForStaticActions;
			if (objectNameParamInfo != null)
			{
				inParamStor.GoDataTop();
				if (inParamStor.Read())
					objectName = (string)inParamStor[objectNameParamInfo.AttribName];
				inParamStor.GoDataTop();
			}
			if (!await _authorizationProvider.HasPermissionAsync(ai, objectName))
				//!!! GetSchema for objectName && put name of object to message
				throw new AuthorizationExceptions(ai.GetAuthorizationExceptionsMessage(objectName));
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
							Strings.TaskPlus_Server_Actions_CreateEvaluator_1
							.ToFormat(ai.InterfaceID, pi.ParameterID, ai.ActionID));
					}
				}
				inParamStor.GoDataTop();
			}
			IAppEvaluator se = ImplementsList.GetTypes(typeof(IAppEvaluator))
				.FirstOrDefault(t => t.FullName.Equals(ai.ClassName, StringComparison.Ordinal))?
				.CreateInstance<IAppEvaluator>();
			return se;
		}
	}
}
