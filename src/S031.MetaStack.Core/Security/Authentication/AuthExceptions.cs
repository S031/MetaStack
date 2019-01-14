using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using System;

#if NETCOREAPP
namespace S031.MetaStack.Core.Security
#else
namespace S031.MetaStack.WinForms.Security
#endif
{
	public class AuthenticationExceptions : Exception
	{
		public AuthenticationExceptions(string message) : base(message)
		{
		}
	}
	public class AuthorizationExceptions : Exception
	{
		/// <summary>
		/// <see cref="S031.MetaStack.Core.App.AuthorizationExceptionsExtension.GetAuthorizationExceptionsMessage(ActionInfo, string)"/>
		/// </summary>
		/// <param name="message"></param>
		public AuthorizationExceptions(string message) : base(message)
		{
		}
	}
}
