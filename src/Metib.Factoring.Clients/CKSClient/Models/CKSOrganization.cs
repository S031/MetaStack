using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSOrganization : CKSSubject
	{
		protected internal CKSOrganization(XElement xsource) : base(xsource)
		{
		}
	}
}
