using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.ORM.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Actions
{
	internal static class ActionsList
	{
		public static MapTable<string, ActionInfo> CreateActionsList()
		{
			MapTable<string, ActionInfo> actions = new MapTable<string, ActionInfo>();
            #region Sys.LoginRequest
            ActionInfo ai = new ActionInfo()
            {
                ActionID = "Sys.LoginRequest",
                AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
                AsyncMode = false,
                AuthenticationRequired = false,
                AuthorizationRequired = false,
                ClassName = typeof(SysLoginRequest).FullName,
                Description = "Запрос на логин (принимает Имя пользователя и открытый ключ клиента, возвращает ИД сессии + открытый ключ сервера)",
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
				AttribName = "LoginInfo",
				FieldName = "LoginInfo",
				ParameterID = "LoginInfo",
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

			#region Sys.Logon
			ai = new ActionInfo()
			{
				ActionID = "Sys.Logon",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = false,
				AuthenticationRequired = false,
				AuthorizationRequired = false,
				ClassName = typeof(SysLogon).FullName,
				Description = "Аутентификация пользователя (принимает Имя пользователя, ИД сессии, зашифрованный открытым ключем сервера пароль, возвращает зашифрованный открытым ключем клиента симметричный ключ)",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = false,
				MultipleRowsResult = false,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Аутентификация пользователя",
				WebAuthentication = ActionWebAuthenticationType.None,
                IsStatic = true
            };
			pi = new ParamInfo()
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
				AttribName = "SessionID",
				FieldName = "SessionID",
				ParameterID = "SessionID",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "ИД сессии",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 34,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "EncryptedKey",
				FieldName = "EncryptedKey",
				ParameterID = "EncryptedKey",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Зашифрованный пароль",
				Position = 3,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "Ticket",
				FieldName = "Ticket",
				ParameterID = "Ticket",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Зашифрованный токен аутентификации",
				Position = 4,
				PresentationType = "TextBox",
				Visible = true,
				Width = 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion Sys.Logon

			#region Sys.Select
			ai = new ActionInfo()
			{
				ActionID = "Sys.Select",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = true,
				AuthenticationRequired = true,
				AuthorizationRequired = true,
				ClassName = typeof(SysSelect).FullName,
				Description = "Выборка данных из источника",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = true,
				MultipleRowsResult = true,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Выборка данных из источника",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			pi = new ParamInfo()
			{
				AttribName = "ParamName",
				FieldName = "ParamName",
				ParameterID = "ParamName",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Наименование параметра",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 30,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "ParamValue",
				FieldName = "ParamValue",
				ParameterID = "ParamValue",
				DataType = "object",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Значение параметра",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 1024,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion

			#region Sys.Getschema
			ai = new ActionInfo()
			{
				ActionID = "Sys.GetSchema",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = true,
				AuthenticationRequired = true,
				AuthorizationRequired = false,
				ClassName = typeof(SysGetSchema).FullName,
				Description = "Получить схему объекта",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = true,
				MultipleRowsResult = true,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Получить схему объекта",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			pi = new ParamInfo()
			{
				AttribName = "ObjectName",
				FieldName = "ObjectName",
				ParameterID = "ObjectName",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Наименование объекта",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 60,
				Required = true,
				IsObjectName = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "ObjectSchema",
				FieldName = "ObjectSchema",
				ParameterID = "ObjectSchema",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Схема объекта",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 1024 * 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion
			
			#region Sys.Logout
			ai = new ActionInfo()
			{
				ActionID = "Sys.Logout",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
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

			#region Sys.SaveSchema
			ai = new ActionInfo()
			{
				ActionID = "Sys.SaveSchema",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = true,
				AuthenticationRequired = true,
				AuthorizationRequired = true,
				ClassName = typeof(SysSaveSchema).FullName,
				Description = "Сохранить схему объекта",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = true,
				MultipleRowsResult = true,
				TransactionSupport = TransactionActionSupport.Support,
				Name = "Сохранить схему объекта",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			pi = new ParamInfo()
			{
				AttribName = "ObjectSchema",
				FieldName = "ObjectSchema",
				ParameterID = "ObjectSchema",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Схема объекта",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 1024 * 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "ObjectSchemaResult",
				FieldName = "ObjectSchema",
				ParameterID = "ObjectSchemaResult",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Схема объекта",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 1024 * 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion

			#region Sys.GetActionInfo
			ai = new ActionInfo()
			{
				ActionID = "Sys.GetActionInfo",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = true,
				AuthenticationRequired = true,
				AuthorizationRequired = true,
				ClassName = typeof(SysGetActionInfo).FullName,
				Description = "Получить описание операции",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = true,
				MultipleRowsResult = true,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Получить описание операции",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			pi = new ParamInfo()
			{
				AttribName = "ActionID",
				FieldName = "ActionID",
				ParameterID = "ActionID",
				DataType = "string",
				Dirrect = ParamDirrect.Input,
				Enabled = true,
				Name = "Наименование операции",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 60,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			pi = new ParamInfo()
			{
				AttribName = "ActionInfo",
				FieldName = "ActionInfo",
				ParameterID = "ActionInfo",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Схема операции",
				Position = 2,
				PresentationType = "TextBox",
				Visible = true,
				Width = 1024 * 256,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion
			
			#region Sys.PipeRead
			ai = new ActionInfo()
			{
				ActionID = "Sys.PipeRead",
				AssemblyID = typeof(ActionsList).Assembly.GetWorkName(),
				AsyncMode = false,
				AuthenticationRequired = true,
				AuthorizationRequired = true,
				ClassName = typeof(SysPipeRead).FullName,
				Description = "Запустить канал вывода информации для текущей сессии",
				EMailOnError = false,
				LogOnError = true,
				MultipleRowsParams = true,
				MultipleRowsResult = true,
				TransactionSupport = TransactionActionSupport.None,
				Name = "Запустить канал вывода информации для текущей сессии",
				WebAuthentication = ActionWebAuthenticationType.Basic,
                IsStatic = true
            };
			pi = new ParamInfo()
			{
				AttribName = "Result",
				FieldName = "Result",
				ParameterID = "Result",
				DataType = "string",
				Dirrect = ParamDirrect.Output,
				Enabled = true,
				Name = "Результат операции",
				Position = 1,
				PresentationType = "TextBox",
				Visible = true,
				Width = 60,
				Required = true
			};
			ai.InterfaceParameters.Add(pi);
			actions.Add(ai.ActionID, ai);
			#endregion

			return actions;
		}
	}
}
