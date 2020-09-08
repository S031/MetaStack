using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlus.Server.Actions
{
	internal static class ActionsListInternal
	{
		public static IDictionary<string, ActionInfo> CreateActionsList()
		{
			MapTable<string, ActionInfo> actions = new MapTable<string, ActionInfo>();

            #region Sys.LoginRequest
            ActionInfo ai = new ActionInfo()
            {
                ActionID = "login",
                AssemblyID = typeof(ActionsListInternal).Assembly.GetWorkName(),
                AsyncMode = true,
                AuthenticationRequired = false,
                AuthorizationRequired = false,
                ClassName = typeof(SysLoginRequest).FullName,
                Description = "Запрос на логин (принимает Имя пользователя и пароль клиента, возвращает JWT токен)",
                EMailOnError = false,
                LogOnError = true,
                MultipleRowsParams = false,
                MultipleRowsResult = false,
                TransactionSupport = TransactionActionSupport.None,
                Name = "Запрос на логин",
                WebAuthentication = ActionWebAuthenticationType.None,
                IsStatic = true
			};
			ParamInfo pi = new ParamInfo()
			{
				AttribName = "UserName",
				FieldName = "UserName",
				ParameterID = "UserName",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Имя пользователя",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 30,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "Password",
				FieldName = "Password",
				ParameterID = "Password",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Пароль клиента",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "JwtToken",
				FieldName = "JwtToken",
				ParameterID = "JwtToken",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Login Info",
				Position = 3,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion Sys.LoginRequest

			#region Sys.Logout
			ai = new ActionInfo()
			{
				ActionID = "Sys.Logout",
				AssemblyID = typeof(ActionsListInternal).Assembly.GetWorkName(),
				AsyncMode = false,
				AuthenticationRequired = false,
				AuthorizationRequired = false,
				ClassName = typeof(SysLogout).FullName,
				Description = "Закрыть сессию",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = false,
				MultipleRowsResult = false,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Получить схему объекта",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			actions.Add(ai.ActionID, ai);
			#endregion

			return actions;
		}
	}
}
