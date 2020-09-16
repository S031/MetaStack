using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSLogin: CKSOperationResult
	{
		public CKSLogin(string loginResponse) : base(loginResponse)
		{
			var elem = Source.Element("return");
			if (Status == CKSOperationStatus.ok)
			{
				UUID = elem.Attribute("UUID").Value;
				foreach (XElement e in Source.Element("session").Element("user").Elements())
					Properties[e.Name.LocalName] = e.Value;
			}
			else
			{
				XElement xerr = elem.Element("error");
				Error = new CKS.CKSOperationError(
					(CKSOperationErrorStatus)Enum.Parse(typeof(CKSOperationErrorStatus), xerr.Attribute("Status").Value),
					xerr.Value);
			}
		}

		public string UUID { get; } = string.Empty;

		IDictionary<string, object> Properties { get; } = new MapTable<string, object>();
	}
}
