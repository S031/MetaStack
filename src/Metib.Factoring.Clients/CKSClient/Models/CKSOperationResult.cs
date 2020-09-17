using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public enum CKSOperationStatus
	{
		ok,
		error,
		pass
	}
	public enum CKSOperationErrorStatus
	{
		parameter,
		security,
		locked,
		busy
	}

	public struct CKSOperationError
	{
		public CKSOperationErrorStatus Status;

		public string ErrorMessage;

		public CKSOperationError(CKSOperationErrorStatus errorStatus, string errorMassge)
		{
			Status = errorStatus;
			ErrorMessage = errorMassge;
		}

	}

	public class CKSOperationResult
	{
		public CKSOperationResult(string cksResponse)
		{
			Source = 
				XDocument.Parse(
					XDocument.Parse(cksResponse)
						.Element("cksOperationPOSTModel")
						.Element("xml").Value)
				.Element("cks");
			var elem = Source.Element("return");
			Status = (CKSOperationStatus)Enum.Parse(typeof(CKSOperationStatus), elem.Attribute("Status").Value);
			if (Status == CKSOperationStatus.error)
			{
				XElement xerr = elem.Element("error");
				Error = new CKS.CKSOperationError(
					(CKSOperationErrorStatus)Enum.Parse(typeof(CKSOperationErrorStatus), xerr.Attribute("Status").Value),
					xerr.Value);
			}
		}
		protected internal XElement Source { get; }

		public CKSOperationStatus Status { get; protected set; }

		public CKSOperationError Error { get; protected set; }

		public override string ToString()
			=> Source.ToString();

		public string this[string path]
			=>Source.Element(path).Value;
	}
}
