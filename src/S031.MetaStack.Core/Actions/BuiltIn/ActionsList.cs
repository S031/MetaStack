using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Actions
{
	internal static class ActionsList
	{
		public static Dictionary<string, ActionInfo> CreateActionsList()
		{
			Dictionary<string, ActionInfo> actions = new Dictionary<string, ActionInfo>();
			ActionInfo ai = new ActionInfo()
			{
				ActionID = "Sys.LoginRequest",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = false,
				AuthenticationRequired = false,
				AuthorizationRequired = false,
				ClassName = typeof(LoginRequest).FullName,
				Description = "Запрос на логин (принимает Имя пользователя и открытый ключ клиента, возвращает открытый ключ сервера)",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = false,
				MultipleRowsResult = false,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Запрос на логин",
				WebAuthentication = ActionWebAuthenticationType.None
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
				AttribName = "PublicKey",
				FieldName = "PublicKey",
				ParameterID = "PublicKey",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Открытый ключ клиента",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "PublicKey",
				FieldName = "PublicKey",
				ParameterID = "PublicKey",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Открытый ключ сервера",
				Position = 3,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			return actions;
		}
	}
}
