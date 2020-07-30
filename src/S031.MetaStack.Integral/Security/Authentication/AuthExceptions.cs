using System;

namespace S031.MetaStack.Security
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
		/// <see cref="MetaStack.Core.App.AuthorizationExceptionsExtension.GetAuthorizationExceptionsMessage(ActionInfo, string)"/>
		/// </summary>
		/// <param name="message"></param>
		public AuthorizationExceptions(string message) : base(message)
		{
		}
	}
}
