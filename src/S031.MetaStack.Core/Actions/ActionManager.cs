using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Security;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using S031.MetaStack.Core.Properties;

namespace S031.MetaStack.Core.Actions
{
	public sealed class ActionManager : ActionManagerBase
	{
		private readonly IServiceProvider _services;
		private readonly ILogger _logger;
		private readonly IAuthorizationProvider _authorizationProvider;

		public ActionManager(IServiceProvider services) 
		{
			_services = services;
			_logger = _services.GetRequiredService<ILogger>();
			_authorizationProvider = _services.GetRequiredService<IAuthorizationProvider>();
			this.SysCatDbContext = _services
				.GetRequiredService<IMdbContextFactory>()
				.GetContext(Strings.SysCatConnection);
		}
		public override DataPackage Execute(ActionInfo ai, DataPackage inParamStor)
		{
			try
			{
				return ExecuteInternal(ai, inParamStor);
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
		private DataPackage ExecuteInternal(ActionInfo ai, DataPackage inParamStor)
		{
			if (ai.AuthorizationRequired)
				Authorize(ai, inParamStor);
			
			var se = CreateEvaluator(ai, inParamStor);
			return se.Invoke(ai, inParamStor);
		}

		private async Task<DataPackage> ExecuteInternalAsync(ActionInfo ai, DataPackage inParamStor)
		{
			if (ai.AuthorizationRequired)
				await AuthorizeAsync(ai, inParamStor);
			
			var se = CreateEvaluator(ai, inParamStor);
			return await se.InvokeAsync(ai, inParamStor);
		}
		private void Authorize(ActionInfo ai, DataPackage inParamStor)
		{
			string objectName = GetObjectNameFromInputParameters(ai, inParamStor);
			if (!_authorizationProvider.HasPermission(ai, objectName))
				//!!! GetSchema for objectName && put name of object to message
				throw new UnauthorizedAccessException(ai.GetAuthorizationExceptionsMessage(objectName));
		}
		private async Task AuthorizeAsync(ActionInfo ai, DataPackage inParamStor)
		{
			string objectName = GetObjectNameFromInputParameters(ai, inParamStor);
			if (!await _authorizationProvider.HasPermissionAsync(ai, objectName))
				//!!! GetSchema for objectName && put name of object to message
				throw new UnauthorizedAccessException(ai.GetAuthorizationExceptionsMessage(objectName));
		}
		private static string GetObjectNameFromInputParameters(ActionInfo ai, DataPackage inParamStor)
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
			return objectName;
		}
		private static readonly MapTable<string, Type> _loadedTypesCache = new MapTable<string, Type>();
		private static IAppEvaluator CreateEvaluator(ActionInfo ai, DataPackage inParamStor)
		{
			var inParams = ai.InterfaceParameters.Where(kvp => kvp.Value.Dirrect == ParamDirrect.Input && kvp.Value.Required).Select(kvp => kvp.Value);
			int pCount = inParams.Count();
			if (inParamStor != null) inParamStor.GoDataTop();
			if (pCount > 0 && (inParamStor == null || inParamStor.FieldCount < pCount || !inParamStor.Read()))
			{
				throw new InvalidOperationException(
					Strings.S031_MetaStack_Core_Actions_CreateEvaluator_2
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
							Strings.S031_MetaStack_Core_Actions_CreateEvaluator_1
							.ToFormat(ai.InterfaceID, pi.ParameterID, ai.ActionID));
					}
				}
				inParamStor.GoDataTop();
			}

			if (!_loadedTypesCache.TryGetValue(ai.ClassName, out Type type))
			{
				var a = LoadAssembly(ai.AssemblyID);
				ImplementsList.Add(typeof(IAppEvaluator), a);
				type = ImplementsList.GetTypes(typeof(IAppEvaluator))
					.FirstOrDefault(t => t.FullName.Equals(ai.ClassName, StringComparison.Ordinal));
				_loadedTypesCache.TryAdd(ai.ClassName, type);
			}
			return type.CreateInstance<IAppEvaluator>();
		}
		private static Assembly LoadAssembly(string assemblyID) => AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));
	}
}
