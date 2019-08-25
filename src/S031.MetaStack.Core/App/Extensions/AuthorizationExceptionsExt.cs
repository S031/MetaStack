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
				return Properties.Strings.S031_MetaStack_Core_SecurityAuthorizationExceptions_GetMessage_1
					.ToFormat(ctx.UserName, ai.Name, objectName);
			return Properties.Strings.S031_MetaStack_Core_SecurityAuthorizationExceptions_GetMessage_1
				.ToFormat(Environment.UserName, ai.Name, objectName);
		}
	}
}
