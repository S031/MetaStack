using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSLogin: CKSBase
	{
		private const string _statusOK = "ok";
		private const string _statusError = "error";

		public CKSLogin(string loginResponse) : base(loginResponse)
		{
			var xresult = Source.Element("cks");
			var elem = xresult.Element("return");
			Status = (CKSOperationStatus)Enum.Parse(typeof(CKSOperationStatus), elem.Attribute("Status").Value);
			if (Status == CKSOperationStatus.ok)
			{
				UUID = elem.Attribute("UUID").Value;
				foreach (XElement e in xresult.Element("session").Element("user").Elements())
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

		Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
	}
}
