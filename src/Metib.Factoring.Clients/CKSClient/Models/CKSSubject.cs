using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSSubject: CKSBase
	{
		private readonly XElement _xsubj;
		public CKSSubject(string cksResponse) : base(cksResponse)
		{
		}

		CKSSubject(XDocument source) : base(source)
		{
			_xsubj = Source.Element("client");
		}

		public int ID => int.Parse(this["id"]);

		public static List<CKSSubject> Load(XDocument source)
		{
			var result = new List<CKS.CKSSubject>();
			var xclients = source.Root.Element("clients");
			foreach (var xclient in xclients.Elements())
				result.Add(new CKSSubject(new XDocument(xclient)));
			return result;
		}
	}
}
