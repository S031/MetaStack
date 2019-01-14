using S031.MetaStack.Core.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.App
{
	public static class AuthorizationExceptionsExtension
	{
		public static string GetAuthorizationExceptionsMessage(this ActionInfo ai, string objectName)
		{
			ActionContext ctx = ai.GetContext();
			if (ctx != null)
				return "S031.MetaStack.Core.SecurityAuthorizationExceptions.GetMessage.1"
					.GetTranslate(ctx.UserName, ai.Name, objectName);
			return "S031.MetaStack.Core.SecurityAuthorizationExceptions.GetMessage.1"
				.GetTranslate(Environment.UserName, ai.Name, objectName);
		}
	}
}
