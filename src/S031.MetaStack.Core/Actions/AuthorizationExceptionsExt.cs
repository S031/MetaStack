using S031.MetaStack.Common;
using System;
using S031.MetaStack.Actions;

namespace S031.MetaStack.Core.Actions
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
