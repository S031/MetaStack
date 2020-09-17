using System;
using System.Collections.Generic;
using System.Text;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSOperationException: InvalidOperationException
	{
		public CKSOperationError ErrorData { get; }

		public CKSOperationException(CKSOperationErrorStatus status, string message)
		{
			ErrorData = new CKSOperationError(status, message);
		}
	}
}
