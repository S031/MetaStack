using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Security
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
