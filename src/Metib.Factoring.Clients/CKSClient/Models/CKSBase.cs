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
	public abstract class CKSBase
	{
		protected CKSBase(XDocument source)
		{
			Source = source;
		}

		protected CKSBase(string cksResponse) : this(Parse(cksResponse))
		{
		}
		public XDocument Source { get; }

		public CKSOperationStatus Status { get; protected set; }

		public CKSOperationError Error { get; protected set; }

		public override string ToString()
			=> Source.ToString();

		public static XDocument Parse(string cksResponse)
			=> XDocument.Parse(
				XDocument.Parse(cksResponse)
					.Element("cksOperationPOSTModel")
					.Element("xml").Value);

		public string this[string path]
			=>Source.Element(path).Value;
	}
}
