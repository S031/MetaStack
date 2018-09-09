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
		public AuthorizationExceptions(string message) : base(message)
		{
		}
	}
}
