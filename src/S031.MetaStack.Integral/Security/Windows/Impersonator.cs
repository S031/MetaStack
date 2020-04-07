using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.Security
{
	public static class Impersonator
	{

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
			int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

		public static T Execute<T>(string userName, string password, Func<T> userAction)
		{
			string domainName = "";
			int pos = userName.IndexOf('\\');
			if (pos > -1)
			{
				domainName = userName.Left(pos);
				userName = userName.Substring(pos + 1);
			}
			else
				domainName = System.Environment.UserDomainName;

			const int LOGON32_PROVIDER_DEFAULT = 0;
			const int LOGON32_LOGON_INTERACTIVE = 2;

			bool returnValue = LogonUser(userName, domainName, password,
				LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
				out SafeAccessTokenHandle safeAccessTokenHandle);

			if (!returnValue)
			{
				int ret = Marshal.GetLastWin32Error();
				Console.WriteLine("LogonUser failed with error code : {0}", ret);
				throw new System.ComponentModel.Win32Exception(ret);
			}

			// Note: if you want to run as unimpersonated, pass
			//       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'
			return WindowsIdentity.RunImpersonated<T>(
				safeAccessTokenHandle,
				userAction);
		}
	}
}